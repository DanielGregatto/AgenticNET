variable "environment"                  { type = string }
variable "project_name"                 { type = string }
variable "resource_group_name"          { type = string }
variable "container_app_environment_id" { type = string }
variable "acr_login_server"             { type = string }
variable "identity_id"                  { type = string }
variable "identity_client_id"           { type = string }
variable "aspnetcore_environment"       { type = string }

# SQL
variable "sql_server_fqdn"              { type = string }
variable "sql_database_name"            { type = string }

# Azure OpenAI
variable "openai_endpoint"              { type = string }
variable "chat_deployment"              { type = string }

# Embeddings (can reuse the same OpenAI resource or a different one)
variable "embedding_endpoint"           { type = string }
variable "embedding_deployment"         { type = string }

# AI Search
variable "search_endpoint"              { type = string }
variable "search_index_name"            { type = string; default = "knowledge" }
variable "search_topk"                  { type = number; default = 5 }
variable "search_vector_field"          { type = string; default = "contentVector" }

# Data Protection & Observability
variable "storage_account_name"         { type = string }
variable "appinsights_connection_string" { type = string; sensitive = true }

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

  secret {
    name  = "appinsights-connection-string"
    value = var.appinsights_connection_string
  }

  template {
    container {
      name   = var.project_name
      image  = "mcr.microsoft.com/azuredocs/containerapps-helloworld:latest"
      cpu    = 0.5
      memory = "1Gi"

      env {
        name  = "ASPNETCORE_ENVIRONMENT"
        value = var.aspnetcore_environment
      }

      # SQL — Managed Identity, no password
      env {
        name  = "ConnectionStrings__DefaultConnection"
        value = "Server=${var.sql_server_fqdn};Database=${var.sql_database_name};Authentication=Active Directory Managed Identity;User Id=${var.identity_client_id};Encrypt=True;MultipleActiveResultSets=True;"
      }

      # Azure OpenAI
      env {
        name  = "AgentOrchestration__Providers__AzureOpenAI__Endpoint"
        value = var.openai_endpoint
      }

      env {
        name  = "AgentOrchestration__DefaultDeployment"
        value = var.chat_deployment
      }

      # Embeddings
      env {
        name  = "Embedding__Endpoint"
        value = var.embedding_endpoint
      }

      env {
        name  = "Embedding__Deployment"
        value = var.embedding_deployment
      }

      # AI Search
      env {
        name  = "RAGSearch__Endpoint"
        value = var.search_endpoint
      }

      env {
        name  = "RAGSearch__IndexName"
        value = var.search_index_name
      }

      env {
        name  = "RAGSearch__TopK"
        value = tostring(var.search_topk)
      }

      env {
        name  = "RAGSearch__VectorFieldName"
        value = var.search_vector_field
      }

      # Data Protection
      env {
        name  = "AzureStorage__AccountName"
        value = var.storage_account_name
      }

      # Application Insights (injected from secret)
      env {
        name        = "ApplicationInsights__ConnectionString"
        secret_name = "appinsights-connection-string"
      }
    }
  }
}
