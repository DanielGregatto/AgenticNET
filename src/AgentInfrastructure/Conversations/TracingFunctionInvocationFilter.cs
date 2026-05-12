using Domain.Contracts.Agent;
using Microsoft.SemanticKernel;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AgentInfrastructure.Conversations
{
    public class TracingFunctionInvocationFilter : IFunctionInvocationFilter
    {
        private readonly TraceContext _traceContext;

        public TracingFunctionInvocationFilter(TraceContext traceContext)
        {
            _traceContext = traceContext;
        }

        public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
        {
            _traceContext.Add(TraceStepType.PluginCall, new
            {
                Plugin = context.Function.PluginName,
                Function = context.Function.Name,
                Input = context.Arguments.ToDictionary(kv => kv.Key, kv => kv.Value?.ToString())
            });

            await next(context);

            var resultStr = context.Result?.GetValue<string>() ?? string.Empty;

            _traceContext.Add(TraceStepType.PluginResult, new
            {
                Plugin = context.Function.PluginName,
                Function = context.Function.Name,
                DocumentsRetrieved = CountDocuments(context, resultStr),
                ResultLength = resultStr.Length
            });
        }

        private static int? CountDocuments(FunctionInvocationContext context, string result)
        {
            if (string.IsNullOrEmpty(result))
                return null;

            var documentSearchPlugins = context.Kernel.Data.TryGetValue("DocumentSearchPlugins", out var v)
                ? v as System.Collections.Generic.HashSet<string>
                : null;

            if (documentSearchPlugins?.Contains(context.Function.PluginName) != true)
                return null;

            return result.Split("---").Length;
        }
    }
}
