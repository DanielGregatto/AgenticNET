// Terraform configuration for deploying an Azure Container App with supporting resources

//Local primeiro faz login na Azure CLI
# az login
# az account set --subscription <SUBSCRIPTION_ID>
# set ARM_SUBSCRIPTION_ID=44<SUBSCRIPTION_ID>

//DEV
# terraform init -backend-config=env/backend.dev.hcl
# terraform plan -var-file=tvars/terraform-dev.tfvars
# terraform apply -var-file=tvars/terraform-dev.tfvars

//PROD
# terraform init -backend-config=env/backend.prod.hcl
# terraform plan -var-file=tvars/terraform-prod.tfvars
# terraform apply -var-file=tvars/terraform-prod.tfvars

//UNLOCK
//terraform force-unlock [ID LOCK]

// Azure Resource Group
module "rg" {
  source      = "./modules/rg"
  environment = var.environment
  location    = var.location
}

module "network" {
  source              = "./modules/network"
  environment         = var.environment
  location            = module.rg.location
  resource_group_name = module.rg.name
}

module "storage" {
  source              = "./modules/storage"
  environment         = var.environment
  project_name        = var.project_name
  location            = module.rg.location
  resource_group_name = module.rg.name
}

module "acr" {
  source              = "./modules/acr"
  environment         = var.environment
  project_name        = var.project_name
  location            = module.rg.location
  resource_group_name = module.rg.name
}

module "logs" {
  source              = "./modules/logs"
  environment         = var.environment
  project_name        = var.project_name
  location            = module.rg.location
  resource_group_name = module.rg.name
}

module "cae" {
  source                    = "./modules/cae"
  environment               = var.environment
  project_name              = var.project_name
  location                  = module.rg.location
  resource_group_name       = module.rg.name
  log_analytics_workspace_id = module.logs.id
  infrastructure_subnet_id  = module.network.subnet_containerapps_id
}

module "identity_acr_pull" {
  source              = "./modules/identity-acr-pull"
  environment         = var.environment
  project_name        = var.project_name
  location            = module.rg.location
  resource_group_name = module.rg.name
  acr_id              = module.acr.id
}

module "api" {
  source                      = "./modules/containerapp-api"
  environment                 = var.environment
  project_name                = var.project_name
  resource_group_name         = module.rg.name
  container_app_environment_id = module.cae.id
  acr_login_server            = module.acr.login_server
  identity_id                 = module.identity_acr_pull.id
  aspnetcore_environment      = var.aspnetcore_environment

  depends_on = [module.identity_acr_pull]
}