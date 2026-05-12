using Domain.Contracts.Agent;
using System;
using System.Collections.Generic;

namespace AgentInfrastructure.Conversations
{
    public class TraceContext
    {
        private readonly List<TraceStep> _steps = new();

        public IReadOnlyList<TraceStep> Steps => _steps.AsReadOnly();

        public void Add(TraceStepType type, object data)
        {
            _steps.Add(new TraceStep
            {
                Type = type,
                Timestamp = DateTimeOffset.UtcNow,
                Data = System.Text.Json.JsonSerializer.Serialize(data)
            });
        }
    }
}
