// Terraform configuration for AgenticNET on Azure Container Apps
//
// Auth: az login && az account set --subscription <SUBSCRIPTION_ID>
//
// DEV:  terraform init -backend-config=env/backend.dev.hcl
//       terraform plan  -var-file=tvars/terraform-dev.tfvars
//       terraform apply -var-file=tvars/terraform-dev.tfvars
//
// PROD: terraform init -backend-config=env/backend.prod.hcl
//       terraform plan  -var-file=tvars/terraform-prod.tfvars
//       terraform apply -var-file=tvars/terraform-prod.tfvars
//
// After first apply — grant the managed identity access to SQL (run once per environment):
//   CREATE USER [uami-<env>-<project>] FROM EXTERNAL PROVIDER;
//   ALTER ROLE db_datareader ADD MEMBER [uami-<env>-<project>];
//   ALTER ROLE db_datawriter ADD MEMBER [uami-<env>-<project>];

module "rg" {
  source      = "./modules/rg"
  environment = var.environment
  location    = var.location
}

module "network" {
  source              = "./modules/network"
  environment         = var.environment
  location            = module.rg.location
  resource_group_name = module.rg.name
}

module "storage" {
  source              = "./modules/storage"
  environment         = var.environment
  project_name        = var.project_name
  location            = module.rg.location
  resource_group_name = module.rg.name
}

module "acr" {
  source              = "./modules/acr"
  environment         = var.environment
  project_name        = var.project_name
  location            = module.rg.location
  resource_group_name = module.rg.name
}

module "logs" {
  source              = "./modules/logs"
  environment         = var.environment
  project_name        = var.project_name
  location            = module.rg.location
  resource_group_name = module.rg.name
}

module "cae" {
  source                     = "./modules/cae"
  environment                = var.environment
  project_name               = var.project_name
  location                   = module.rg.location
  resource_group_name        = module.rg.name
  log_analytics_workspace_id = module.logs.id
  infrastructure_subnet_id   = module.network.subnet_containerapps_id
}

module "openai" {
  source              = "./modules/openai"
  environment         = var.environment
  project_name        = var.project_name
  location            = var.openai_location
  resource_group_name = module.rg.name

  chat_deployment_name      = var.chat_deployment_name
  chat_model_name           = var.chat_model_name
  chat_model_version        = var.chat_model_version

  embedding_deployment_name = var.embedding_deployment_name
  embedding_model_name      = var.embedding_model_name
  embedding_model_version   = var.embedding_model_version
}

module "search" {
  source              = "./modules/search"
  environment         = var.environment
  project_name        = var.project_name
  location            = module.rg.location
  resource_group_name = module.rg.name
  sku                 = var.search_sku
}

module "identity_acr_pull" {
  source              = "./modules/identity-acr-pull"
  environment         = var.environment
  project_name        = var.project_name
  location            = module.rg.location
  resource_group_name = module.rg.name
  acr_id              = module.acr.id
  storage_account_id  = module.storage.id
  openai_resource_id  = module.openai.id
  search_resource_id  = module.search.id
}

module "sql" {
  source              = "./modules/sql"
  environment         = var.environment
  project_name        = var.project_name
  location            = module.rg.location
  resource_group_name = module.rg.name
  database_name       = var.sql_database_name
  sku_name            = var.sql_sku

  # UAMI is the AAD admin — no SQL password exists
  aad_admin_object_id = module.identity_acr_pull.principal_id
  aad_admin_login     = "uami-${var.environment}-${var.project_name}"

  depends_on = [module.identity_acr_pull]
}

module "api" {
  source                       = "./modules/containerapp-api"
  environment                  = var.environment
  project_name                 = var.project_name
  resource_group_name          = module.rg.name
  container_app_environment_id = module.cae.id
  acr_login_server             = module.acr.login_server
  identity_id                  = module.identity_acr_pull.id
  identity_client_id           = module.identity_acr_pull.client_id
  aspnetcore_environment       = var.aspnetcore_environment

  sql_server_fqdn  = module.sql.server_fqdn
  sql_database_name = module.sql.database_name

  openai_endpoint  = module.openai.endpoint
  chat_deployment  = module.openai.chat_deployment

  embedding_endpoint  = module.openai.endpoint
  embedding_deployment = module.openai.embedding_deployment

  search_endpoint     = module.search.endpoint
  search_index_name   = var.search_index_name
  search_topk         = var.search_topk
  search_vector_field = var.search_vector_field

  storage_account_name         = module.storage.name
  appinsights_connection_string = module.logs.appinsights_connection_string

  depends_on = [module.identity_acr_pull, module.sql]
}
