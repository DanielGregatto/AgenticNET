variable "environment"         { type = string }
variable "project_name"        { type = string }
variable "location"            { type = string }
variable "resource_group_name" { type = string }
variable "aad_admin_login"     { type = string }
variable "aad_admin_object_id" { type = string }
variable "uami_principal_id"   { type = string }
variable "tags" {
  type    = map(string)
  default = {}
}

variable "database_name" {
  type    = string
  default = ""
}

variable "sku_name" {
  type    = string
  default = "GP_S_Gen5_1"
}

variable "min_capacity" {
  type    = number
  default = 0.5
}

variable "auto_pause_delay_in_minutes" {
  type    = number
  default = 60
}

locals {
  db_name    = var.database_name != "" ? var.database_name : "${var.project_name}-${var.environment}"
  serverless = startswith(var.sku_name, "GP_S_")
}

resource "random_password" "sql_admin" {
  length           = 24
  special          = true
  override_special = "!#$%&*?"
}

resource "azurerm_mssql_server" "this" {
  name                         = "sql-${var.environment}-${var.project_name}"
  resource_group_name          = var.resource_group_name
  location                     = var.location
  version                      = "12.0"
  minimum_tls_version          = "1.2"
  administrator_login          = "sqladmin"
  administrator_login_password = random_password.sql_admin.result
  tags                         = var.tags

  azuread_administrator {
    login_username              = var.aad_admin_login
    object_id                   = var.aad_admin_object_id
    azuread_authentication_only = false
  }
}

resource "azurerm_mssql_database" "this" {
  name                 = local.db_name
  server_id            = azurerm_mssql_server.this.id
  collation            = "SQL_Latin1_General_CP1_CI_AS"
  sku_name             = var.sku_name
  storage_account_type = "Geo"
  tags                 = var.tags

  min_capacity                = local.serverless ? var.min_capacity : null
  auto_pause_delay_in_minutes = local.serverless ? var.auto_pause_delay_in_minutes : null

  lifecycle {
    prevent_destroy = false
  }
}

resource "azurerm_mssql_firewall_rule" "azure_services" {
  name             = "AllowAzureServices"
  server_id        = azurerm_mssql_server.this.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}
