environment            = "dev"
project_name           = "agenticnet"
aspnetcore_environment = "Development"
location               = "brazilsouth"
openai_location        = "eastus2"

sql_database_name = "agenticnet-dev"
sql_sku           = "Basic"

# Deployment name = your custom label; model name = Azure model identifier
chat_deployment_name  = "chat"
chat_model_name       = "gpt-4o-mini"
chat_model_version    = "2024-07-18"

embedding_deployment_name = "embeddings"
embedding_model_name      = "text-embedding-ada-002"
embedding_model_version   = "2"

search_sku          = "standard"
search_index_name   = "knowledge"
search_topk         = 5
search_vector_field = "contentVector"
