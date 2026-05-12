using System.Text.Json.Serialization;

namespace AgentInfrastructure.Orchestration
{
    internal class ReviewerOutput
    {
        [JsonPropertyName("isValid")]
        public bool IsValid { get; set; }

        [JsonPropertyName("confidence")]
        public double Confidence { get; set; }

        [JsonPropertyName("notes")]
        public string Notes { get; set; }
    }
}
