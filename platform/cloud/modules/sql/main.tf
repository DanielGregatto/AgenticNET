variable "environment"         { type = string }
variable "project_name"        { type = string }
variable "location"            { type = string }
variable "resource_group_name" { type = string }
variable "aad_admin_object_id" { type = string }
variable "aad_admin_login"     { type = string }
variable "database_name"       { type = string; default = "" }
variable "sku_name"            { type = string; default = "Basic" }

locals {
  db_name = var.database_name != "" ? var.database_name : "${var.project_name}-${var.environment}"
}

resource "azurerm_mssql_server" "this" {
  name                = "sql-${var.environment}-${var.project_name}"
  resource_group_name = var.resource_group_name
  location            = var.location
  version             = "12.0"
  minimum_tls_version = "1.2"

  # Entra ID only — SQL password auth disabled
  azuread_administrator {
    login_username              = var.aad_admin_login
    object_id                   = var.aad_admin_object_id
    azuread_authentication_only = true
  }
}

resource "azurerm_mssql_database" "this" {
  name      = local.db_name
  server_id = azurerm_mssql_server.this.id
  collation = "SQL_Latin1_General_CP1_CI_AS"
  sku_name  = var.sku_name

  lifecycle {
    prevent_destroy = true
  }
}

# Required for Azure-hosted services (Container Apps) to reach SQL without VNet peering
resource "azurerm_mssql_firewall_rule" "azure_services" {
  name             = "AllowAzureServices"
  server_id        = azurerm_mssql_server.this.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}
