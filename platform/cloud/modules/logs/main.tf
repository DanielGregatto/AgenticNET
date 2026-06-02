variable "environment"         { type = string }
variable "project_name"        { type = string }
variable "location"            { type = string }
variable "resource_group_name" { type = string }
variable "tags" {
  type    = map(string)
  default = {}
}

resource "azurerm_log_analytics_workspace" "this" {
  name                = "log-${var.environment}-${var.project_name}"
  location            = var.location
  resource_group_name = var.resource_group_name
  retention_in_days   = 30
  tags                = var.tags
}

resource "azurerm_application_insights" "this" {
  name                = "appi-${var.environment}-${var.project_name}"
  location            = var.location
  resource_group_name = var.resource_group_name
  workspace_id        = azurerm_log_analytics_workspace.this.id
  application_type    = "web"
  tags                = var.tags
}
