variable "environment"         { type = string }
variable "project_name"        { type = string }
variable "location"            { type = string }
variable "resource_group_name" { type = string }
variable "acr_id"              { type = string }
variable "storage_account_id"  { type = string }

# Optional: set these once the OpenAI and Search modules are created
variable "openai_resource_id"  { type = string; default = "" }
variable "search_resource_id"  { type = string; default = "" }

resource "azurerm_user_assigned_identity" "this" {
  name                = "uami-${var.environment}-${var.project_name}"
  location            = var.location
  resource_group_name = var.resource_group_name
}

resource "azurerm_role_assignment" "acr_pull" {
  scope                = var.acr_id
  role_definition_name = "AcrPull"
  principal_id         = azurerm_user_assigned_identity.this.principal_id
}

resource "azurerm_role_assignment" "storage_blob_contributor" {
  scope                = var.storage_account_id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = azurerm_user_assigned_identity.this.principal_id
}

resource "azurerm_role_assignment" "openai_user" {
  count                = var.openai_resource_id != "" ? 1 : 0
  scope                = var.openai_resource_id
  role_definition_name = "Cognitive Services OpenAI User"
  principal_id         = azurerm_user_assigned_identity.this.principal_id
}

resource "azurerm_role_assignment" "search_index_reader" {
  count                = var.search_resource_id != "" ? 1 : 0
  scope                = var.search_resource_id
  role_definition_name = "Search Index Data Reader"
  principal_id         = azurerm_user_assigned_identity.this.principal_id
}

resource "azurerm_role_assignment" "search_index_contributor" {
  count                = var.search_resource_id != "" ? 1 : 0
  scope                = var.search_resource_id
  role_definition_name = "Search Index Data Contributor"
  principal_id         = azurerm_user_assigned_identity.this.principal_id
}
