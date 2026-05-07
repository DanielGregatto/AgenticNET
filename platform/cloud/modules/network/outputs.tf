output "vnet_name"            { value = azurerm_virtual_network.this.name }
output "subnet_app_id"        { value = azurerm_subnet.app.id }
output "subnet_containerapps_id" { value = azurerm_subnet.containerapps.id }
