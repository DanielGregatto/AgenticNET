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

variable "admin_group_object_id" {
  type        = string
  description = "Object ID of the AAD admin group (developers + CI/CD SP). Members get Cognitive Services User on this resource."
}

variable "deployments" {
  type = list(object({
    name                   = string
    format                 = optional(string, "OpenAI")
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
  name                = "ai-${var.environment}-${var.project_name}${local.suffix}"
  resource_group_name = var.resource_group_name
  location            = var.location
  kind                = "AIServices"
  sku_name            = "S0"

  custom_subdomain_name         = "ai-${var.environment}-${var.project_name}${local.suffix}"
  local_auth_enabled            = false
  public_network_access_enabled = true
  project_management_enabled    = true
  tags                          = var.tags

  identity {
    type = "SystemAssigned"
  }
}

resource "azurerm_role_assignment" "admin_group_ai_user" {
  scope                = azurerm_cognitive_account.this.id
  role_definition_name = "Cognitive Services User"
  principal_id         = var.admin_group_object_id
}

resource "azurerm_cognitive_deployment" "this" {
  for_each               = local.deployments_map
  name                   = each.key
  cognitive_account_id   = azurerm_cognitive_account.this.id
  version_upgrade_option = each.value.version_upgrade_option

  model {
    format  = each.value.format
    name    = each.value.model_name
    version = each.value.model_version
  }

  sku {
    name     = each.value.sku_name
    capacity = each.value.capacity
  }
}
