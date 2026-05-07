using System.Collections.Generic;

namespace Services.Contracts.Results
{
    public record AgentListItemResult(
        string Name,
        string Description,
        string Provider,
        string DeploymentOrModel,
        List<string> Plugins);
}
