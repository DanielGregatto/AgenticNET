output "id"              { value = azurerm_cognitive_account.this.id }
output "endpoint"       { value = azurerm_cognitive_account.this.endpoint }
output "foundry_endpoint" { value = "https://${azurerm_cognitive_account.this.custom_subdomain_name}.services.ai.azure.com/" }

output "deployments" {
  description = "Map of deployment name to deployment name, for referencing in app config."
  value       = { for k, v in azurerm_cognitive_deployment.this : k => v.name }
}
