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
}
