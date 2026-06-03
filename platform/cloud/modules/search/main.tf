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
variable "sku" {
  type    = string
  default = "standard"
}
# Needed so the indexer can read blob storage via its system identity
variable "storage_account_id" { type = string }

locals {
  suffix = var.name_suffix != "" ? "-${var.name_suffix}" : ""
}

resource "azurerm_search_service" "this" {
  name                = "srch-${var.environment}-${var.project_name}${local.suffix}"
  resource_group_name = var.resource_group_name
  location            = var.location
  sku                 = var.sku
  tags                = var.tags

  local_authentication_enabled = false

  identity {
    type = "SystemAssigned"
  }
}

# Grant the search service's identity read access to blob so indexers can pull documents
resource "azurerm_role_assignment" "search_blob_reader" {
  scope                = var.storage_account_id
  role_definition_name = "Storage Blob Data Reader"
  principal_id         = azurerm_search_service.this.identity[0].principal_id
}
