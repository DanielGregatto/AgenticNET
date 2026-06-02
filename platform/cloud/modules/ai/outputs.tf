output "id"       { value = azurerm_cognitive_account.this.id }
output "endpoint" { value = azurerm_cognitive_account.this.endpoint }

output "deployments" {
  description = "Map of deployment name to deployment name, for referencing in app config."
  value       = { for k, v in azurerm_cognitive_deployment.this : k => v.name }
}
