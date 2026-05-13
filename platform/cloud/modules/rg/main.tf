variable "environment"  { type = string }
variable "project_name" { type = string }
variable "location"     { type = string }
variable "tags" {
  type    = map(string)
  default = {}
}

resource "azurerm_resource_group" "this" {
  name     = "rg-${var.environment}-${var.project_name}"
  location = var.location
  tags     = var.tags
}