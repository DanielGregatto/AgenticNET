output "acr_login_server" {
  value = module.acr.login_server
}

output "container_app_name" {
  value = module.api.name
}

output "resource_group_name" {
  value = module.rg.name
}

output "project_name" {
  value = var.project_name
}
