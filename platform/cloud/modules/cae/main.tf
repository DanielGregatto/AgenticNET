variable "environment"              { type = string }
variable "project_name"             { type = string }
variable "location"                 { type = string }
variable "resource_group_name"      { type = string }
variable "log_analytics_workspace_id" { type = string }
variable "infrastructure_subnet_id" { type = string }

resource "azurerm_container_app_environment" "this" {
  name                       = "cae-${var.environment}-${var.project_name}"
  location                   = var.location
  resource_group_name        = var.resource_group_name
  log_analytics_workspace_id = var.log_analytics_workspace_id

  infrastructure_subnet_id   = var.infrastructure_subnet_id
}
