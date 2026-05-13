variable "environment" {
  type    = string
  default = "dev"
}

variable "project_name" {
  type    = string
  default = "agenticnet"
}

variable "location" {
  type    = string
  default = "brazilsouth"
}

# Use a different location for OpenAI if your target models are only available in specific regions
variable "openai_location" {
  type    = string
  default = "eastus2"
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

# Azure OpenAI model deployments
variable "chat_deployment_name" {
  type    = string
  default = "gpt-4o"
}

variable "chat_model_name" {
  type    = string
  default = "gpt-4o"
}

variable "chat_model_version" {
  type    = string
  default = "2024-11-20"
}

variable "embedding_deployment_name" {
  type    = string
  default = "text-embedding-ada-002"
}

variable "embedding_model_name" {
  type    = string
  default = "text-embedding-ada-002"
}

variable "embedding_model_version" {
  type    = string
  default = "2"
}

# AI Search
variable "search_sku" {
  type    = string
  default = "standard"
}

variable "search_index_name" {
  type    = string
  default = "knowledge"
}

variable "search_topk" {
  type    = number
  default = 5
}

variable "search_vector_field" {
  type    = string
  default = "contentVector"
}

variable "tags" {
  type    = map(string)
  default = {}
}
