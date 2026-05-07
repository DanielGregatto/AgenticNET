using Domain.Configs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace UI.API.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class ApiKeyAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private const string ApiKeyHeaderName = "apikey";

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var config = context.HttpContext.RequestServices
                             .GetService(typeof(IOptions<SecretEndpointConfig>)) as IOptions<SecretEndpointConfig>;

            if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
            {
                context.Result = new ContentResult()
                {
                    StatusCode = 401,
                    Content = "API Key is missing"
                };
                return;
            }

            if (config == null || config.Value.APIKey != extractedApiKey)
            {
                context.Result = new ContentResult()
                {
                    StatusCode = 403,
                    Content = "Invalid API Key"
                };
                return;
            }

            await Task.CompletedTask;
        }
    }
}