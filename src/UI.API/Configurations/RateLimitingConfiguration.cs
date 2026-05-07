using System.Security.Claims;
using System.Threading.RateLimiting;
using Domain.Configs;
using Microsoft.Extensions.Options;

namespace UI.API.Configurations
{
    public static class RateLimitingConfiguration
    {
        public static void AddRateLimiterConfiguration(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                {
                    var rateLimitConfig = httpContext.RequestServices.GetRequiredService<IOptions<RateLimitConfig>>().Value;
                    var headers = httpContext.Request.Headers;

                    var isBypass = !string.IsNullOrEmpty(rateLimitConfig.BypassToken)
                                   && headers.TryGetValue(rateLimitConfig.BypassHeaderName, out var bypass)
                                   && bypass.ToString().ToLowerInvariant() == rateLimitConfig.BypassToken.ToLowerInvariant();

                    var userId = httpContext.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                 ?? httpContext.User?.Identity?.Name
                                 ?? headers["Authorization"].ToString();

                    var key = string.IsNullOrWhiteSpace(userId)
                        ? httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"
                        : userId;

                    return isBypass
                        ? RateLimitPartition.GetTokenBucketLimiter(key, _ => new TokenBucketRateLimiterOptions
                        {
                            TokenLimit = rateLimitConfig.BypassTokenLimit,
                            TokensPerPeriod = rateLimitConfig.BypassTokensPerPeriod,
                            ReplenishmentPeriod = TimeSpan.FromSeconds(rateLimitConfig.BypassReplenishmentPeriodSeconds),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0,
                            AutoReplenishment = true
                        })
                        : RateLimitPartition.GetTokenBucketLimiter(key, _ => new TokenBucketRateLimiterOptions
                        {
                            TokenLimit = rateLimitConfig.DefaultTokenLimit,
                            TokensPerPeriod = rateLimitConfig.DefaultTokensPerPeriod,
                            ReplenishmentPeriod = TimeSpan.FromSeconds(rateLimitConfig.DefaultReplenishmentPeriodSeconds),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0,
                            AutoReplenishment = true
                        });
                });

                options.RejectionStatusCode = 429;

                options.OnRejected = async (context, token) =>
                {
                    context.HttpContext.Response.StatusCode = 429;
                    context.HttpContext.Response.ContentType = "application/json";
                    await context.HttpContext.Response.WriteAsync("""
                {
                    "error": "Rate limit exceeded",
                    "hint": "Please wait before trying again."
                }
                """, token);
                };
            });
        }
    }
}
