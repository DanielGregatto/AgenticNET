# Usage: powershell -ExecutionPolicy Bypass -File .\platform\scripts\setup-azure.ps1 dev
#        powershell -ExecutionPolicy Bypass -File .\platform\scripts\setup-azure.ps1 prod
#
# What this script does:
#   Gives GitHub Actions permission to deploy AgenticNET to Azure without
#   storing any passwords. It uses OIDC — GitHub proves its identity to Azure
#   via a short-lived token tied to a specific branch, so nothing expires
#   or needs manual rotation.
#
#   Fully automated — creates everything on both Azure and GitHub sides:
#   Azure : App Registration, Service Principal, OIDC Federated Credential, Role Assignments
#   GitHub: Environment + all required secrets
#
# Run once per environment before your first deploy.
# Safe to re-run if something fails — all steps are idempotent.
#
# Prerequisites:
#   - Azure CLI installed and logged in  (run: az login)
#   - GitHub CLI installed and logged in (run: gh auth login)
#   - PowerShell Core on Linux/Mac       (https://aka.ms/install-powershell)

param(
    [Parameter(Position = 0)]
    [ValidateSet("dev", "prod")]
    [string]$Env
)

if (-not $Env) {
    Write-Host ""
    Write-Host "Usage: .\platform\scripts\setup-azure.ps1 [dev|prod]"
    Write-Host ""
    Write-Host "  dev  -> configures the 'development' environment"
    Write-Host "          pipeline triggers on merges to the 'development' branch"
    Write-Host ""
    Write-Host "  prod -> configures the 'production' environment"
    Write-Host "          pipeline triggers on merges to the 'main' branch"
    exit 1
}

# ── Prerequisites ─────────────────────────────────────────────────────────────

Write-Host ""
Write-Host "Checking prerequisites..."

if (-not (Get-Command az -ErrorAction SilentlyContinue)) {
    Write-Host ""
    Write-Host "  ERROR: Azure CLI not found."
    Write-Host "  Install it from https://learn.microsoft.com/cli/azure/install-azure-cli"
    exit 1
}

if (-not (Get-Command gh -ErrorAction SilentlyContinue)) {
    Write-Host ""
    Write-Host "  ERROR: GitHub CLI not found."
    Write-Host "  Install it from https://cli.github.com  then run: gh auth login"
    exit 1
}

$CurrentAzureUser = az account show --query user.name -o tsv 2>$null
if (-not $CurrentAzureUser) {
    Write-Host ""
    Write-Host "  ERROR: Not logged in to Azure. Run: az login"
    exit 1
}

$CurrentGitHubUser = gh api user --jq .login 2>$null
if (-not $CurrentGitHubUser) {
    Write-Host ""
    Write-Host "  ERROR: Not logged in to GitHub CLI. Run: gh auth login"
    exit 1
}

Write-Host "  OK Azure CLI  -- logged in as: $CurrentAzureUser"
Write-Host "  OK GitHub CLI -- logged in as: $CurrentGitHubUser"

# ── Parse config files ────────────────────────────────────────────────────────

$ScriptDir   = Split-Path -Parent $MyInvocation.MyCommand.Path
$RepoRoot    = Resolve-Path (Join-Path $ScriptDir "..\..")
$TfVarsFile  = Join-Path $RepoRoot "platform/cloud/tvars/terraform-$Env.tfvars"
$BackendFile = Join-Path $RepoRoot "platform/cloud/env/backend.$Env.hcl"

function Parse-ConfigValue($File, $Key) {
    $line = Get-Content $File | Where-Object { $_ -match "^$Key\s*=" }
    if ($line) { return ($line -replace '.*=\s*"(.*)"\s*$', '$1').Trim() }
}

$TfStateRG       = Parse-ConfigValue $BackendFile "resource_group_name"
$TfContainerName = Parse-ConfigValue $BackendFile "container_name"
$TfStateLocation = Parse-ConfigValue $TfVarsFile "location"
if (-not $TfStateLocation) { $TfStateLocation = "eastus2" }

if ($Env -eq "dev") {
    $Branch        = "development"
    $GitHubEnvName = "development"
} else {
    $Branch        = "main"
    $GitHubEnvName = "production"
}

$SubscriptionId   = az account show --query id       -o tsv
$SubscriptionName = az account show --query name     -o tsv
$TenantId         = az account show --query tenantId -o tsv
$SqlObjectId      = az ad signed-in-user show --query id -o tsv 2>$null

$GitHubRepo = ""
try {
    $remote = git -C $RepoRoot remote get-url origin 2>$null
    if ($remote -match "github\.com[:/](.+?)(?:\.git)?$") {
        $GitHubRepo = $Matches[1]
    }
} catch {}

if (-not $GitHubRepo) {
    Write-Host ""
    Write-Host "  Could not detect GitHub repo from git remote."
    Write-Host "  This is needed to scope the OIDC trust to your specific repository."
    $GitHubRepo = Read-Host "  Enter GitHub repo (e.g. DanielGregatto/AgenticNET)"
}

Write-Host "  OK GitHub repo  : $GitHubRepo"
Write-Host "  OK Subscription : $SubscriptionName ($SubscriptionId)"

# ── Name suffix ───────────────────────────────────────────────────────────────

Write-Host ""
Write-Host "  Enter the resource name suffix for the '$Env' environment."
Write-Host "  This is appended to globally unique Azure resource names (ACR, SQL, OpenAI, Search)."
Write-Host "  Max 6 characters. Pick any short unique value (e.g. abc123)."

$NameSuffix = ""
while ($NameSuffix.Length -eq 0 -or $NameSuffix.Length -gt 6) {
    if ($NameSuffix.Length -gt 6) {
        Write-Host "  '$NameSuffix' is $($NameSuffix.Length) characters -- must be 6 or fewer. Try again."
    }
    $NameSuffix = Read-Host "  TF_NAME_SUFFIX (max 6 chars)"
}

# Derive storage account name from suffix and update the backend HCL file so
# 'terraform init' uses the correct account for this user's deployment.
$TfStateSA       = "stterraformagnet$NameSuffix"
$TfBackendSuffix = $NameSuffix

(Get-Content $BackendFile) `
    -replace 'storage_account_name\s*=\s*"[^"]*"', "storage_account_name = `"$TfStateSA`"" |
    Set-Content $BackendFile -Encoding utf8
Write-Host "  Updated $(Split-Path $BackendFile -Leaf) -> storage_account_name = `"$TfStateSA`""

$AppName     = "agenticnet-github-$Env"
$OidcSubject = "repo:${GitHubRepo}:environment:${GitHubEnvName}"

# ── Summary ───────────────────────────────────────────────────────────────────

Write-Host ""
Write-Host "=========================================================="
Write-Host "  AgenticNET OIDC Setup -- environment: $Env"
Write-Host "  The following resources will be created in the next steps:"
Write-Host "=========================================================="
Write-Host ""
Write-Host "  Terraform backend storage:"
Write-Host "  * Resource group '$TfStateRG' in $TfStateLocation (created if it does not exist)"
Write-Host "  * Storage account '$TfStateSA' with container '$TfContainerName' (created if it does not exist)"
Write-Host ""
Write-Host "  Azure (AD + RBAC):"
Write-Host "  * App Registration '$AppName' -- the identity GitHub Actions will use"
Write-Host "  * Federated Credential -- trusts tokens from branch '$Branch' in $GitHubRepo only"
Write-Host "  * Contributor on subscription -- lets Terraform create and manage all resources"
Write-Host "  * User Access Administrator on subscription -- lets Terraform assign roles to the managed identity"
Write-Host "  * Storage Blob Data Contributor on $TfStateSA -- lets Terraform read/write state"
Write-Host ""
Write-Host "  GitHub:"
Write-Host "  * Environment '$GitHubEnvName' (created if it does not exist)"
Write-Host "  * 6 environment secrets: AZURE_CLIENT_ID, AZURE_TENANT_ID, AZURE_SUBSCRIPTION_ID,"
Write-Host "    SQL_AAD_ADMIN_LOGIN, SQL_AAD_ADMIN_OBJECT_ID, TF_NAME_SUFFIX ($NameSuffix)"
Write-Host "  * 1 repo-level secret: TF_BACKEND_SUFFIX ($TfBackendSuffix) -- shared between dev and prod"
Write-Host ""
$Confirm = Read-Host "  Proceed? [y/N]"
if ($Confirm.ToLower() -ne "y") {
    Write-Host "  Aborted."
    exit 0
}

# ── 1. Terraform Backend ──────────────────────────────────────────────────────

Write-Host ""
Write-Host "-- [1/7] Terraform Backend -----------------------------------"
Write-Host "   Creating the resource group and storage account that hold"
Write-Host "   Terraform state. This must exist before 'terraform init' can run."
Write-Host ""

$ExistingRG = az group show --name $TfStateRG --query name -o tsv 2>$null
if ($ExistingRG -and $ExistingRG -ne "None") {
    Write-Host "   Resource group '$TfStateRG' already exists -- skipping."
} else {
    az group create --name $TfStateRG --location $TfStateLocation --output none
    Write-Host "   Created resource group '$TfStateRG' in $TfStateLocation"
}

$ExistingSA = az storage account show --name $TfStateSA --resource-group $TfStateRG --query name -o tsv 2>$null
if ($ExistingSA -and $ExistingSA -ne "None") {
    Write-Host "   Storage account '$TfStateSA' already exists -- skipping."
} else {
    az storage account create `
        --name $TfStateSA `
        --resource-group $TfStateRG `
        --location $TfStateLocation `
        --sku Standard_LRS `
        --kind StorageV2 `
        --allow-blob-public-access false `
        --min-tls-version TLS1_2 `
        --output none
    Write-Host "   Created storage account '$TfStateSA'"
}

$ExistingContainer = az storage container show --name $TfContainerName --account-name $TfStateSA --auth-mode login --query name -o tsv 2>$null
if ($ExistingContainer -and $ExistingContainer -ne "None") {
    Write-Host "   Container '$TfContainerName' already exists -- skipping."
} else {
    az storage container create --name $TfContainerName --account-name $TfStateSA --auth-mode login --output none
    Write-Host "   Created blob container '$TfContainerName'"
}

# ── 2. App Registration ───────────────────────────────────────────────────────

Write-Host ""
Write-Host "-- [2/7] App Registration -----------------------------------"
Write-Host "   Creating an identity in Azure AD that GitHub Actions will use."
Write-Host ""

$ExistingAppId = az ad app list --display-name $AppName --query "[0].appId" -o tsv 2>$null
if ($ExistingAppId -and $ExistingAppId -ne "None") {
    $AppId = $ExistingAppId
    Write-Host "   Already exists -- skipping creation."
} else {
    $AppId = az ad app create --display-name $AppName --query appId -o tsv
    Write-Host "   Created"
}
Write-Host "   App ID: $AppId"

# ── 2. Service Principal ──────────────────────────────────────────────────────

Write-Host ""
Write-Host "-- [3/7] Service Principal ----------------------------------"
Write-Host "   Linking the App Registration to Azure RBAC so roles can be assigned."
Write-Host ""

$ExistingSp = az ad sp list --filter "appId eq '$AppId'" --query "[0].id" -o tsv 2>$null
if ($ExistingSp -and $ExistingSp -ne "None") {
    $SpObjectId = $ExistingSp
    Write-Host "   Already exists -- skipping creation."
} else {
    $SpObjectId = az ad sp create --id $AppId --query id -o tsv
    Write-Host "   Created"
}
Write-Host "   SP Object ID: $SpObjectId"

# ── 3. Federated Credential ───────────────────────────────────────────────────

Write-Host ""
Write-Host "-- [4/7] OIDC Federated Credential --------------------------"
Write-Host "   This is the trust rule that makes OIDC work."
Write-Host "   Azure will only accept GitHub tokens from environment '$GitHubEnvName'"
Write-Host "   in repo '$GitHubRepo'. Any other environment or fork is rejected."
Write-Host ""

# Remove stale branch-scoped credential if it exists (created by older versions of this script)
$OldSubject = "repo:${GitHubRepo}:ref:refs/heads/${Branch}"
$StaleFed = az ad app federated-credential list --id $AppId `
    --query "[?subject=='$OldSubject'].id" -o tsv 2>$null
if ($StaleFed -and $StaleFed -ne "None") {
    az ad app federated-credential delete --id $AppId --federated-credential-id $StaleFed --output none
    Write-Host "   Removed stale branch-scoped credential."
}

$ExistingFed = az ad app federated-credential list --id $AppId `
    --query "[?subject=='$OidcSubject'].id" -o tsv 2>$null
if ($ExistingFed -and $ExistingFed -ne "None") {
    Write-Host "   Already exists -- skipping creation."
} else {
    $FedJson = [ordered]@{
        name      = "github-env-$GitHubEnvName"
        issuer    = "https://token.actions.githubusercontent.com"
        subject   = $OidcSubject
        audiences = @("api://AzureADTokenExchange")
    } | ConvertTo-Json

    $TempFile = [System.IO.Path]::GetTempFileName()
    Set-Content -Path $TempFile -Value $FedJson -Encoding utf8
    az ad app federated-credential create --id $AppId --parameters "@$TempFile" --output none
    Remove-Item -Path $TempFile -Force
    Write-Host "   Created"
}
Write-Host "   Subject: $OidcSubject"

# ── 4. Role Assignments ───────────────────────────────────────────────────────

Write-Host ""
Write-Host "-- [5/7] Role Assignments ------------------------------------"
Write-Host ""

function Assign-AzureRole($Role, $Scope, $Label, $Reason) {
    Write-Host "   Assigning: $Label"
    Write-Host "   Why: $Reason"
    az role assignment create `
        --assignee-object-id $SpObjectId `
        --assignee-principal-type ServicePrincipal `
        --role $Role `
        --scope $Scope `
        --output none 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   Done"
    } else {
        Write-Host "   Already assigned (skipping)"
    }
    Write-Host ""
}

$SubScope = "/subscriptions/$SubscriptionId"
$SaScope  = "$SubScope/resourceGroups/$TfStateRG/providers/Microsoft.Storage/storageAccounts/$TfStateSA"

Assign-AzureRole `
    "Contributor" `
    $SubScope `
    "Contributor on subscription" `
    "Terraform needs to create the resource group and all resources inside it. Subscription-level scope avoids a chicken-and-egg problem where the resource group must exist before the role can be assigned to it."

Assign-AzureRole `
    "User Access Administrator" `
    $SubScope `
    "User Access Administrator on subscription" `
    "Terraform's identity module assigns RBAC roles to the managed identity (AcrPull, OpenAI User, Search Reader, etc.). Contributor alone cannot perform Microsoft.Authorization/roleAssignments/write."

Assign-AzureRole `
    "Storage Blob Data Contributor" `
    $SaScope `
    "Storage Blob Data Contributor on $TfStateSA" `
    "Terraform stores its state (.tfstate) in this storage account. Without this, Terraform cannot track what it has already deployed and will recreate everything on each run."

# ── 5. GitHub Environment ─────────────────────────────────────────────────────

Write-Host "-- [6/7] GitHub Environment ----------------------------------"
Write-Host "   Creating environment '$GitHubEnvName' in $GitHubRepo."
Write-Host "   The workflow file references this name -- it must exist before the first run."
Write-Host ""

gh api "repos/$GitHubRepo/environments/$GitHubEnvName" --method PUT --field wait_timer=0 --silent
if ($LASTEXITCODE -eq 0) {
    Write-Host "   Environment '$GitHubEnvName' ready"
} else {
    Write-Host "   Could not create environment (check repo permissions)"
}

# ── 6. GitHub Secrets ─────────────────────────────────────────────────────────

Write-Host ""
Write-Host "-- [7/7] GitHub Secrets --------------------------------------"
Write-Host "   Setting secrets encrypted at rest, never exposed in logs."
Write-Host ""
Write-Host "   Environment secrets (visible only to the '$GitHubEnvName' pipeline):"

function Set-EnvSecret($Name, $Value) {
    if (-not $Value) { Write-Host "   SKIP $Name -- value is empty"; return }
    gh secret set $Name --repo $GitHubRepo --env $GitHubEnvName --body $Value
    if ($LASTEXITCODE -eq 0) { Write-Host "   OK $Name" } else { Write-Host "   FAILED $Name" }
}

function Set-RepoSecret($Name, $Value) {
    if (-not $Value) { Write-Host "   SKIP $Name -- value is empty"; return }
    gh secret set $Name --repo $GitHubRepo --body $Value
    if ($LASTEXITCODE -eq 0) { Write-Host "   OK $Name" } else { Write-Host "   FAILED $Name" }
}

Set-EnvSecret "AZURE_CLIENT_ID"         $AppId
Set-EnvSecret "AZURE_TENANT_ID"         $TenantId
Set-EnvSecret "AZURE_SUBSCRIPTION_ID"   $SubscriptionId
Set-EnvSecret "SQL_AAD_ADMIN_LOGIN"     $CurrentAzureUser
Set-EnvSecret "SQL_AAD_ADMIN_OBJECT_ID" $SqlObjectId
Set-EnvSecret "TF_NAME_SUFFIX"          $NameSuffix

Write-Host ""
Write-Host "   Repo-level secret (shared between dev and prod pipelines):"
Set-RepoSecret "TF_BACKEND_SUFFIX" $TfBackendSuffix

# ── Done ──────────────────────────────────────────────────────────────────────

Write-Host ""
Write-Host "=========================================================="
Write-Host "  Setup complete for environment: $Env"
Write-Host "=========================================================="
Write-Host ""
Write-Host "  Created in Azure:"
Write-Host "  * TF backend RG    : $TfStateRG"
Write-Host "  * TF backend SA    : $TfStateSA (container: $TfContainerName)"
Write-Host "  * App Registration : $AppName ($AppId)"
Write-Host "  * Federated trust  : branch '$Branch' in $GitHubRepo"
Write-Host "  * Roles            : Contributor (subscription), Blob Data Contributor ($TfStateSA)"
Write-Host ""
Write-Host "  Created in GitHub:"
Write-Host "  * Environment        : $GitHubEnvName"
Write-Host "  * Environment secrets: AZURE_CLIENT_ID, AZURE_TENANT_ID, AZURE_SUBSCRIPTION_ID,"
Write-Host "                         SQL_AAD_ADMIN_LOGIN, SQL_AAD_ADMIN_OBJECT_ID, TF_NAME_SUFFIX ($NameSuffix)"
Write-Host "  * Repo-level secret  : TF_BACKEND_SUFFIX ($TfBackendSuffix)"
Write-Host ""
Write-Host "  Verify at: https://github.com/$GitHubRepo/settings/environments"
Write-Host ""

if ($Env -eq "dev") {
    Write-Host "  Next: run the same script for prod when you are ready:"
    Write-Host "  .\platform\scripts\setup-azure.ps1 prod"
    Write-Host ""
    Write-Host "  Then merge a PR to 'development' and the pipeline will run automatically."
} else {
    Write-Host "  Merge a PR to 'main' and the production pipeline will run automatically."
}
Write-Host ""
