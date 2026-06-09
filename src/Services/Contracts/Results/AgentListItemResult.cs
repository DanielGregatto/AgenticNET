using System.Collections.Generic;

namespace Services.Contracts.Results
{
    /// <summary>Summary of a registered agent returned by <c>GET /api/v1/agents</c>.</summary>
    public record AgentListItemResult(
        /// <summary>Agent name as declared in <c>AgentOrchestration:Agents[].Name</c>.</summary>
        string Name,
        /// <summary>Human-readable description of the agent's purpose.</summary>
        string Description,
        /// <summary>Azure AI provider key (e.g. "AzureAI", "AzureAIFoundry").</summary>
        string Provider,
        /// <summary>Model deployment name used by this agent.</summary>
        string DeploymentOrModel,
        /// <summary>Plugins loaded for this agent (e.g. "RAG:Suppliers", "ProductCatalog").</summary>
        List<string> Plugins);
}
