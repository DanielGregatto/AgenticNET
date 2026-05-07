output "id" { value = azurerm_user_assigned_identity.this.id }
output "principal_id" { value = azurerm_user_assigned_identity.this.principal_id }
output "role_assignment_id" { value = azurerm_role_assignment.acr_pull.id }
