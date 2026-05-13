variable "environment"         { type = string }
variable "project_name"        { type = string }
variable "location"            { type = string }
variable "resource_group_name" { type = string }
variable "tags" {
  type    = map(string)
  default = {}
}

variable "chat_deployment_name" {
  type    = string
  default = "chat"
}

variable "chat_model_name" {
  type    = string
  default = "gpt-4o"
}

variable "chat_model_version" {
  type    = string
  default = "2024-11-20"
}

variable "chat_capacity" {
  type    = number
  default = 10
}

variable "embedding_deployment_name" {
  type    = string
  default = "embeddings"
}

variable "embedding_model_name" {
  type    = string
  default = "text-embedding-ada-002"
}

variable "embedding_model_version" {
  type    = string
  default = "2"
}

variable "embedding_capacity" {
  type    = number
  default = 10
}

resource "azurerm_cognitive_account" "this" {
  name                = "oai-${var.environment}-${var.project_name}"
  resource_group_name = var.resource_group_name
  location            = var.location
  kind                = "OpenAI"
  sku_name            = "S0"

  custom_subdomain_name         = "oai-${var.environment}-${var.project_name}"
  local_auth_enabled            = false
  public_network_access_enabled = true
  tags                          = var.tags
}

resource "azurerm_cognitive_deployment" "chat" {
  name                   = var.chat_deployment_name
  cognitive_account_id   = azurerm_cognitive_account.this.id
  version_upgrade_option = "OnceNewDefaultVersionAvailable"

  model {
    format  = "OpenAI"
    name    = var.chat_model_name
    version = var.chat_model_version
  }

  sku {
    name     = "GlobalStandard"
    capacity = var.chat_capacity
  }
}

resource "azurerm_cognitive_deployment" "embedding" {
  name                   = var.embedding_deployment_name
  cognitive_account_id   = azurerm_cognitive_account.this.id
  version_upgrade_option = "OnceNewDefaultVersionAvailable"

  model {
    format  = "OpenAI"
    name    = var.embedding_model_name
    version = var.embedding_model_version
  }

  sku {
    name     = "Standard"
    capacity = var.embedding_capacity
  }
}
