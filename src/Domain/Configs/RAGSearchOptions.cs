namespace Domain.Configs
{
    public class RAGSearchOptions
    {
        public const string SectionName = "RAGSearch";

        public string Endpoint { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string IndexName { get; set; } = "knowledge";
        public int TopK { get; set; } = 5;
        public string VectorFieldName { get; set; } = "contentVector";
    }
}
