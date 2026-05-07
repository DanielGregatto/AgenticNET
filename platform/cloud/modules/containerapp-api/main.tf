variable "environment"                  { type = string }
variable "project_name"                 { type = string }
variable "resource_group_name"          { type = string }
variable "container_app_environment_id" { type = string }
variable "acr_login_server"             { type = string }
variable "identity_id"                  { type = string }
variable "aspnetcore_environment"       { type = string }

resource "azurerm_container_app" "this" {
  name                         = "ca-${var.environment}-${var.project_name}"
  resource_group_name          = var.resource_group_name
  container_app_environment_id = var.container_app_environment_id
  revision_mode                = "Single"

  identity {
    type         = "UserAssigned"
    identity_ids = [var.identity_id]
  }

  ingress {
    external_enabled = true
    target_port      = 8080
    transport        = "auto"

    traffic_weight {
      latest_revision = true
      percentage      = 100
    }
  }

  registry {
    server   = var.acr_login_server
    identity = var.identity_id
  }

  template {
    container {
      name   = var.project_name
      image  = "mcr.microsoft.com/azuredocs/containerapps-helloworld:latest"
      cpu    = 0.25
      memory = "0.5Gi"

      env {
        name  = "ASPNETCORE_ENVIRONMENT"
        value = var.aspnetcore_environment
      }
    }
  }
}
