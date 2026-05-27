output "id"                            { value = azurerm_log_analytics_workspace.this.id }
output "appinsights_connection_string"  {
  value     = azurerm_application_insights.this.connection_string
  sensitive = true
}
