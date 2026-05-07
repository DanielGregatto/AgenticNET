using Data.Context;
using Domain.Contracts.Common;
using Domain.Interfaces;
using MediatR;
using Services.Contracts.Results;
using Services.Core;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Features.Agent.Queries.ListAgents
{
    public class ListAgentsQueryHandler : BaseQueryHandler,
        IRequestHandler<ListAgentsQuery, Result<IEnumerable<AgentListItemResult>>>
    {
        private readonly IAgentOrchestrator _orchestrator;

        public ListAgentsQueryHandler(AppDbContext context, IUser user, IAgentOrchestrator orchestrator)
            : base(context, user)
        {
            _orchestrator = orchestrator;
        }

        public Task<Result<IEnumerable<AgentListItemResult>>> Handle(
            ListAgentsQuery request,
            CancellationToken cancellationToken)
        {
            var agents = _orchestrator.GetRegisteredAgents()
                .Select(a => new AgentListItemResult(
                    a.Name,
                    a.Description,
                    a.Provider,
                    a.DeploymentOrModel,
                    a.Plugins))
                .ToList();

            return Task.FromResult(
                Result<IEnumerable<AgentListItemResult>>.Success(agents));
        }
    }
}
