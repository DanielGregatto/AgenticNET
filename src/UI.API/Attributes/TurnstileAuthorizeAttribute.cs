using Domain.Configs;
using Domain.Contracts.API;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Services.Interfaces;

namespace UI.API.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class TurnstileAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private const string TurnstileHeaderName = "X-Turnstile-Token";
        private const string ApiKeyHeaderName = "apikey";

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var httpContext = context.HttpContext;
            var validator = httpContext.RequestServices.GetService<ITurnstileValidatorService>();
            var config = httpContext.RequestServices
                             .GetService(typeof(IOptions<SecretEndpointConfig>)) as IOptions<SecretEndpointConfig>;
            var errors = new List<ErrorResponseItemDto>();
            bool validatedThroughApiKey = false;

            //Se possuir apikey ignora o turnstile e tenta validar
            if (httpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
                if (config != null && config.Value.APIKey == extractedApiKey)
                    validatedThroughApiKey = true;

            if (!validatedThroughApiKey)
            {
                if (!httpContext.Request.Headers.TryGetValue(TurnstileHeaderName, out var token) || string.IsNullOrWhiteSpace(token))
                {
                    errors.Add(new ErrorResponseItemDto("Turnstile", "Token do Turnstile está ausente."));
                }
                else if (validator == null || !await validator.ValidateAsync(token!))
                {
                    errors.Add(new ErrorResponseItemDto("Turnstile", "Token do Turnstile é inválido ou expirado."));
                }

                if (errors.Any())
                {
                    var errorResponse = new ErrorResponseDto(
                        type: "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                        title: "Erro na validação do Turnstile",
                        status: 400,
                        traceId: Guid.NewGuid().ToString(),
                        items: errors
                    );

                    context.Result = new BadRequestObjectResult(errorResponse);
                }
            }

            await Task.CompletedTask;
        }
    }
}