namespace Domain.Configs
{
    public class EmbeddingOptions
    {
        public const string SectionName = "Embedding";

        public string Endpoint { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string Deployment { get; set; } = "text-embedding-ada-002";
    }
}
