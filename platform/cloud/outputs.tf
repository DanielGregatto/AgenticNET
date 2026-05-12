output "resource_group_name" {
  value = module.rg.name
}

output "acr_login_server" {
  value = module.acr.login_server
}

output "container_app_name" {
  value = module.api.name
}

output "sql_server_fqdn" {
  value = module.sql.server_fqdn
}

output "openai_endpoint" {
  value = module.openai.endpoint
}

output "search_endpoint" {
  value = module.search.endpoint
}

output "identity_client_id" {
  value = module.identity_acr_pull.client_id
}

output "project_name" {
  value = var.project_name
}
