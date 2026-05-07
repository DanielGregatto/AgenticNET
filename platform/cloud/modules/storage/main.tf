variable "environment"         { type = string }
variable "project_name"        { type = string }
variable "location"            { type = string }
variable "resource_group_name" { type = string }

locals {
  storage_app_name = "str${var.environment}${var.project_name}"
}

resource "azurerm_storage_account" "this" {
  name                     = local.storage_app_name
  resource_group_name      = var.resource_group_name
  location                 = var.location
  account_tier             = "Standard"
  account_replication_type = "LRS"

  lifecycle {
    prevent_destroy = true
  }
}
