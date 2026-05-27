// Terraform configuration for AgenticNET on Azure Container Apps
//
// Auth: az login && az account set --subscription <SUBSCRIPTION_ID>
///set ARM_SUBSCRIPTION_ID=<SUBSCRIPTION_ID>
//
// DEV:  terraform init -backend-config=env/backend.dev.hcl
//       terraform plan  -var-file=tvars/terraform-dev.tfvars
//       terraform apply -var-file=tvars/terraform-dev.tfvars
//
// PROD: terraform init -backend-config=env/backend.prod.hcl
//       terraform plan  -var-file=tvars/terraform-prod.tfvars
//       terraform apply -var-file=tvars/terraform-prod.tfvars
//
// After first apply — connect to SQL as the AAD admin and run once per environment:
//   CREATE USER [uami-<env>-<project>] FROM EXTERNAL PROVIDER;
//   ALTER ROLE db_owner ADD MEMBER [uami-<env>-<project>];
// (db_owner required — app runs EF Core migrations which need DDL permissions)

locals {
  tags            = merge({
    environment = var.environment
    project     = var.project_name
    managed_by  = "terraform"
  }, var.tags)
  sql_location    = var.sql_location    != "" ? var.sql_location    : var.location
  search_location = var.search_location != "" ? var.search_location : var.location
}

module "rg" {
  source       = "./modules/rg"
  environment  = var.environment
  project_name = var.project_name
  name_suffix  = var.name_suffix
  location     = var.location
  tags         = local.tags
}

module "network" {
  source              = "./modules/network"
  environment         = var.environment
  project_name        = var.project_name
  location            = module.rg.location
  resource_group_name = module.rg.name
  tags                = local.tags
}

module "storage" {
  source              = "./modules/storage"
  environment         = var.environment
  project_name        = var.project_name
  location            = module.rg.location
  resource_group_name = module.rg.name
  name_suffix         = var.name_suffix
  tags                = local.tags
}

module "acr" {
  source              = "./modules/acr"
  environment         = var.environment
  project_name        = var.project_name
  location            = module.rg.location
  resource_group_name = module.rg.name
  name_suffix         = var.name_suffix
  tags                = local.tags
}

module "logs" {
  source              = "./modules/logs"
  environment         = var.environment
  project_name        = var.project_name
  location            = module.rg.location
  resource_group_name = module.rg.name
  tags                = local.tags
}

module "cae" {
  source                     = "./modules/cae"
  environment                = var.environment
  project_name               = var.project_name
  location                   = module.rg.location
  resource_group_name        = module.rg.name
  log_analytics_workspace_id = module.logs.id
  infrastructure_subnet_id   = module.network.subnet_containerapps_id
  tags                       = local.tags
}

module "openai" {
  source              = "./modules/openai"
  environment         = var.environment
  project_name        = var.project_name
  location            = var.openai_location
  resource_group_name = module.rg.name
  name_suffix         = var.name_suffix

  chat_deployment_name      = var.chat_deployment_name
  chat_model_name           = var.chat_model_name
  chat_model_version        = var.chat_model_version

  embedding_deployment_name = var.embedding_deployment_name
  embedding_model_name      = var.embedding_model_name
  embedding_model_version   = var.embedding_model_version
  tags                      = local.tags
}

module "search" {
  source              = "./modules/search"
  environment         = var.environment
  project_name        = var.project_name
  location            = local.search_location
  resource_group_name = module.rg.name
  name_suffix         = var.name_suffix
  sku                 = var.search_sku
  tags                = local.tags
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
  tags                = local.tags
}

resource "time_sleep" "identity_propagation" {
  depends_on      = [module.identity_acr_pull]
  create_duration = "30s"
}

module "sql" {
  source              = "./modules/sql"
  environment         = var.environment
  project_name        = var.project_name
  location            = local.sql_location
  resource_group_name = module.rg.name
  name_suffix         = var.name_suffix
  database_name               = var.sql_database_name
  sku_name                    = var.sql_sku
  min_capacity                = var.sql_min_capacity
  auto_pause_delay_in_minutes = var.sql_auto_pause_delay_in_minutes

  aad_admin_login     = var.sql_aad_admin_login
  aad_admin_object_id = var.sql_aad_admin_object_id
  uami_principal_id   = module.identity_acr_pull.principal_id

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

  embedding_endpoint  = module.openai.endpoint
  embedding_deployment = module.openai.embedding_deployment

  search_endpoint     = module.search.endpoint
  search_index_name   = var.search_index_name
  search_topk         = var.search_topk
  search_vector_field = var.search_vector_field

  storage_account_name          = module.storage.name
  appinsights_connection_string = module.logs.appinsights_connection_string
  tags                          = local.tags

  depends_on = [time_sleep.identity_propagation, module.sql]
}