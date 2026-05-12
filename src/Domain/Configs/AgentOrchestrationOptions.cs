using System.Collections.Generic;

namespace Domain.Configs
{
    public class AgentOrchestrationOptions
    {
        public const string SectionName = "AgentOrchestration";

        public string DefaultProvider { get; set; } = "AzureOpenAI";
        public string RouterAgentName { get; set; } = "RouterAgent";
        public string DefaultAgentName { get; set; } = "GeneralAdvisor";
        public Dictionary<string, AgentProviderOptions> Providers { get; set; } = new();
        public List<AgentOptions> Agents { get; set; } = new();
    }
}
