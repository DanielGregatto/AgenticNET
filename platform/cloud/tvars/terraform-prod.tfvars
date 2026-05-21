environment            = "prod"
name_suffix            = "051433"
project_name           = "agenticnet"
aspnetcore_environment = "Production"
location               = "eastus2"
openai_location        = "eastus2"
sql_location           = "westus"

sql_database_name               = "agenticnet"
sql_sku                         = "GP_S_Gen5_1"
sql_min_capacity                = 0.5
sql_auto_pause_delay_in_minutes = 30

# Deployment name = your custom label; model name = Azure model identifier
chat_deployment_name  = "chat"
chat_model_name       = "gpt-4o"
chat_model_version    = "2024-11-20"

embedding_deployment_name = "embeddings"
embedding_model_name      = "text-embedding-ada-002"
embedding_model_version   = "2"

search_sku          = "free"
search_index_name   = "knowledge"
search_topk         = 5
search_vector_field = "contentVector"
