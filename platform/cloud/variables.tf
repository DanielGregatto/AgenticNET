variable "environment" {
  type    = string
  default = "dev"
}

variable "name_suffix" {
  type        = string
  default     = ""
  description = "Short unique suffix appended to globally unique resource names (storage, ACR, SQL, OpenAI, search). Required when deploying to a shared subscription. Example: 'abc123'"
}

variable "project_name" {
  type    = string
  default = "agenticnet"
}

variable "location" {
  type    = string
  default = "brazilsouth"
}

# Use a different location for AI Services if your target models are only available in specific regions
variable "ai_location" {
  type    = string
  default = "eastus2"
}

# SQL Server provisioning is restricted in some regions — override if needed
variable "sql_location" {
  type    = string
  default = ""
}

# AI Search availability varies by region — override if needed
variable "search_location" {
  type    = string
  default = ""
}

variable "aspnetcore_environment" {
  type    = string
  default = "Development"
}

# SQL
variable "sql_database_name" {
  type    = string
  default = ""
}

variable "sql_local_dev_ip" {
  type        = string
  default     = ""
  description = "Developer's public IP to whitelist in the SQL firewall. Written by setup-azure.ps1."
}

variable "sql_aad_admin_login" {
  type        = string
  description = "AAD login (email) of the SQL server administrator. Used to grant interactive access for running migrations and management queries."
}

variable "sql_aad_admin_object_id" {
  type        = string
  description = "AAD object ID of the SQL server administrator. Run: az ad signed-in-user show --query id -o tsv"
}

variable "sql_sku" {
  type    = string
  default = "GP_S_Gen5_1"
}

variable "sql_min_capacity" {
  type    = number
  default = 0.5
}

variable "sql_auto_pause_delay_in_minutes" {
  type    = number
  default = 60
}

# Azure AI Services model deployments
variable "ai_deployments" {
  type = list(object({
    name                   = string
    format                 = optional(string, "OpenAI")
    model_name             = string
    model_version          = string
    capacity               = number
    sku_name               = optional(string, "GlobalStandard")
    version_upgrade_option = optional(string, "OnceNewDefaultVersionAvailable")
  }))
  default     = []
  description = "List of model deployments to provision inside the Azure AI Services resource. Each entry creates one deployment. Use 'format' to specify the model provider (e.g. OpenAI, DeepSeek, Anthropic)."
}

# AI Search
variable "search_sku" {
  type    = string
  default = "standard"
}

variable "search_topk" {
  type    = number
  default = 5
}

variable "tags" {
  type    = map(string)
  default = {}
}

variable "destroy_environment" {
  type        = bool
  default     = false
  description = "Set to true to destroy all resources. Pipeline runs terraform destroy instead of apply."
}
