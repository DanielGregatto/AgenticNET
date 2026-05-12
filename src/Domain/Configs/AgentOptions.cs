using System.Collections.Generic;

namespace Domain.Configs
{
    public class AgentOptions
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string SystemPrompt { get; set; }

        /// <summary>Provider key matching a key in Providers dictionary. Falls back to DefaultProvider when empty.</summary>
        public string Provider { get; set; }

        /// <summary>Deployment name (AzureOpenAI) or model ID (OpenAI), e.g. "gpt-4o".</summary>
        public string DeploymentOrModel { get; set; }

        /// <summary>Plugin names this agent is allowed to call. Resolved at registration time.</summary>
        public List<string> Plugins { get; set; } = new();

        public double Temperature { get; set; } = 0.7;
        public int MaxTokens { get; set; } = 2000;
        public AgentReviewOptions Review { get; set; } = new();
    }
}
