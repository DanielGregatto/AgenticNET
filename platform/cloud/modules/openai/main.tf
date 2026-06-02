variable "environment"         { type = string }
variable "project_name"        { type = string }
variable "location"            { type = string }
variable "resource_group_name" { type = string }
variable "name_suffix" {
  type    = string
  default = ""
}
variable "tags" {
  type    = map(string)
  default = {}
}

variable "deployments" {
  type = list(object({
    name                   = string
    model_name             = string
    model_version          = string
    capacity               = number
    sku_name               = optional(string, "GlobalStandard")
    version_upgrade_option = optional(string, "OnceNewDefaultVersionAvailable")
  }))
}

locals {
  suffix          = var.name_suffix != "" ? "-${var.name_suffix}" : ""
  deployments_map = { for d in var.deployments : d.name => d }
}

resource "azurerm_cognitive_account" "this" {
  name                = "oai-${var.environment}-${var.project_name}${local.suffix}"
  resource_group_name = var.resource_group_name
  location            = var.location
  kind                = "OpenAI"
  sku_name            = "S0"

  custom_subdomain_name         = "oai-${var.environment}-${var.project_name}${local.suffix}"
  local_auth_enabled            = false
  public_network_access_enabled = true
  tags                          = var.tags
}

resource "azurerm_cognitive_deployment" "this" {
  for_each               = local.deployments_map
  name                   = each.key
  cognitive_account_id   = azurerm_cognitive_account.this.id
  version_upgrade_option = each.value.version_upgrade_option

  model {
    format  = "OpenAI"
    name    = each.value.model_name
    version = each.value.model_version
  }

  sku {
    name     = each.value.sku_name
    capacity = each.value.capacity
  }
}

# State migration: map old named resources to the new for_each addresses.
# These can be removed once all environments have been applied with the new config.
moved {
  from = azurerm_cognitive_deployment.chat
  to   = azurerm_cognitive_deployment.this["chat"]
}

moved {
  from = azurerm_cognitive_deployment.embedding
  to   = azurerm_cognitive_deployment.this["embeddings"]
}
