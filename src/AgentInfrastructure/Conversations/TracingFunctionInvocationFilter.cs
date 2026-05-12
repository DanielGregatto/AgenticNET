using Domain.Contracts.Agent;
using Microsoft.SemanticKernel;
using System;
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
                Input = context.Arguments.ToString()
            });

            await next(context);

            var resultStr = context.Result?.GetValue<string>() ?? string.Empty;

            _traceContext.Add(TraceStepType.PluginResult, new
            {
                Plugin = context.Function.PluginName,
                Function = context.Function.Name,
                DocumentsRetrieved = CountDocuments(context.Function.Name, resultStr),
                ResultLength = resultStr.Length
            });
        }

        private static int? CountDocuments(string functionName, string result)
        {
            if (functionName != "SearchKnowledgeBase" || string.IsNullOrEmpty(result))
                return null;

            return result.Split("---").Length;
        }
    }
}
