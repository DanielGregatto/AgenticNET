namespace Domain.Configs
{
    public class AgentReviewOptions
    {
        public bool Required { get; set; }
        public string AgentReviewerName { get; set; }

        /// <summary>Minimum acceptable confidence score (0.0–1.0). Null means no threshold check.</summary>
        public double? ConfidenceScore { get; set; }
    }
}
