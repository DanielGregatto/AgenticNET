environment            = "prod"
project_name           = "agenticnet"
aspnetcore_environment = "Production"
location               = "eastus2"
openai_location        = "eastus2"
sql_location           = "westus"

sql_database_name               = "agenticnet"
sql_sku                         = "GP_S_Gen5_1"
sql_min_capacity                = 0.5
sql_auto_pause_delay_in_minutes = 30

openai_deployments = [
  {
    name          = "chat"
    model_name    = "gpt-4o-mini"
    model_version = "2024-07-18"
    capacity      = 10
    sku_name      = "GlobalStandard"
  },
  {
    name          = "embeddings"
    model_name    = "text-embedding-ada-002"
    model_version = "2"
    capacity      = 10
    sku_name      = "Standard"
  },
]

search_sku          = "free"
search_index_name   = "knowledge"
search_topk         = 5
search_vector_field = "contentVector"
