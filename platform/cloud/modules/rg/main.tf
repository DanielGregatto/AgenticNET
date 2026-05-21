variable "environment"  { type = string }
variable "project_name" { type = string }
variable "location"     { type = string }
variable "name_suffix" {
  type    = string
  default = ""
}
variable "tags" {
  type    = map(string)
  default = {}
}

resource "azurerm_resource_group" "this" {
  name     = "rg-${var.environment}-${var.project_name}-${var.name_suffix}"
  location = var.location
  tags     = var.tags
}