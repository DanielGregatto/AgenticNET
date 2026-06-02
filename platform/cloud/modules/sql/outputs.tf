output "server_id"      { value = azurerm_mssql_server.this.id }
output "server_fqdn"    { value = azurerm_mssql_server.this.fully_qualified_domain_name }
output "database_name"  { value = azurerm_mssql_database.this.name }
output "sql_admin_login" { value = "sqladmin" }

output "sql_admin_password" {
  value     = random_password.sql_admin.result
  sensitive = true
}

output "server_principal_id" {
  value = azurerm_mssql_server.this.identity[0].principal_id
}
