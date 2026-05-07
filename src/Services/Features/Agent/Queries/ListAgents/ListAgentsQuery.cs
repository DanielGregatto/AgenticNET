using Domain.Contracts.Common;
using MediatR;
using Services.Contracts.Results;
using System.Collections.Generic;

namespace Services.Features.Agent.Queries.ListAgents
{
    public class ListAgentsQuery : IRequest<Result<IEnumerable<AgentListItemResult>>>
    {
    }
}
