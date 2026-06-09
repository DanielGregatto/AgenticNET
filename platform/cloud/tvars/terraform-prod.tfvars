environment            = "prod"
sql_local_dev_ip       = "191.9.111.54" # set by setup-azure.ps1
project_name           = "agenticnet"
aspnetcore_environment = "Production"
location               = "eastus"
ai_location            = "eastus2"
sql_location           = "westus"

sql_database_name               = "agenticnet"
sql_sku                         = "GP_S_Gen5_1"
sql_min_capacity                = 0.5
sql_auto_pause_delay_in_minutes = 30

ai_deployments = [
  {
    name          = "chat"
    format        = "OpenAI"
    model_name    = "gpt-4o"
    model_version = "2024-11-20"
    capacity      = 10
    sku_name      = "GlobalStandard"
  },
  {
    name          = "embeddings"
    format        = "OpenAI"
    model_name    = "text-embedding-ada-002"
    model_version = "2"
    capacity      = 10
    sku_name      = "Standard"
  },
  {
    name          = "deepseek-r1"
    format        = "DeepSeek"
    model_name    = "DeepSeek-R1"
    model_version = "1"
    capacity      = 1
    sku_name      = "GlobalStandard"
  },
]

search_location = "eastus"
search_sku      = "basic"
search_topk     = 5

# Set to true and push to master to run a full terraform destroy instead of apply
destroy_environment = false
