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

output "sql_admin_login" {
  value = module.sql.sql_admin_login
}

output "sql_admin_password" {
  value     = module.sql.sql_admin_password
  sensitive = true
}

output "ai_endpoint" {
  value = module.ai.endpoint
}

output "search_endpoint" {
  value = module.search.endpoint
}

output "identity_client_id" {
  value = module.identity_acr_pull.client_id
}

output "uami_name" {
  value = module.identity_acr_pull.name
}

output "sql_database_name" {
  value = module.sql.database_name
}

output "sql_server_principal_id" {
  value = module.sql.server_principal_id
}

output "project_name" {
  value = var.project_name
}
