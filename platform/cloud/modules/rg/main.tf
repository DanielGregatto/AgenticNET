variable "environment" { type = string }
variable "location"    { type = string }

resource "azurerm_resource_group" "this" {
  name     = "rg-${var.environment}"
  location = var.location
}