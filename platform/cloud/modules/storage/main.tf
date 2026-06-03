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

locals {
  storage_app_name = "str${var.environment}${var.project_name}${var.name_suffix}"
}

resource "azurerm_storage_account" "this" {
  name                     = local.storage_app_name
  resource_group_name      = var.resource_group_name
  location                 = var.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
  tags                     = var.tags

  share_properties {
    retention_policy {
      days = 7
    }
  }

  lifecycle {
    prevent_destroy = false
  }
}

resource "azurerm_storage_container" "keys" {
  name                  = "keys"
  storage_account_id    = azurerm_storage_account.this.id
  container_access_type = "private"
}

# Knowledge base documents — subfolders (suppliers/, faq/) map to AI Search indexes
resource "azurerm_storage_container" "documents" {
  name                  = "documents"
  storage_account_id    = azurerm_storage_account.this.id
  container_access_type = "private"
}
