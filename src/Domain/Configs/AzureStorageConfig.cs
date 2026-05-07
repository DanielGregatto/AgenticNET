namespace Domain.Configs
{
    public class AzureStorageConfig
    {
        public string ConnectionString { get; set; }
        public string Container { get; set; }
        public string BlobName { get; set; }
    }
}
