using System.Collections.Generic;

namespace Domain.Configs
{
    public class RAGSearchOptions
    {
        public const string SectionName = "RAGSearch";

        public string Endpoint { get; set; } = string.Empty;
        public int TopK { get; set; } = 5;
        public string VectorFieldName { get; set; } = "text_vector";

        /// <summary>Registered knowledge base catalogs. Drives index creation in CI/CD and validates RAG plugin keys at runtime.</summary>
        public List<string> Catalogs { get; set; } = new();
    }
}
