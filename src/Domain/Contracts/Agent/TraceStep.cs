using System;

namespace Domain.Contracts.Agent
{
    /// <summary>A single step in the agent execution trace.</summary>
    public class TraceStep
    {
        /// <summary>
        /// Step category:
        /// <c>RouterDecision</c> - which agent was selected and why;
        /// <c>PluginCall</c> - function/plugin invoked by the agent;
        /// <c>PluginResult</c> - raw output returned by the plugin;
        /// <c>ReviewerDecision</c> - confidence score and whether a retry was triggered.
        /// </summary>
        public TraceStepType Type { get; set; }

        /// <summary>UTC timestamp when this step occurred.</summary>
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>JSON-serialized step payload. Schema varies by <c>Type</c>.</summary>
        public string Data { get; set; }
    }
}
