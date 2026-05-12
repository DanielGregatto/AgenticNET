using System;

namespace Domain.Contracts.Agent
{
    public class TraceStep
    {
        public TraceStepType Type { get; set; }
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>JSON-serialized step payload. Schema varies by Type.</summary>
        public string Data { get; set; }
    }
}
