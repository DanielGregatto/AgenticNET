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

variable "sql_sku" {
  type    = string
  default = "Basic"
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
