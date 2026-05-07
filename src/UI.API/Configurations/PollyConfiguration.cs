using Polly;

namespace UI.API.Configurations
{
    public static class PollyConfiguration
    {
        private static ILogger _logger;

        public static void ConfigureLogger(ILogger logger)
        {
            _logger = logger;
        }

        public static void AddPollyResilience(this IServiceCollection services)
        {
            services.AddHttpClient("DefaultHttpClient")
                .ConfigureHttpClient(c =>
                {
                    c.Timeout = Timeout.InfiniteTimeSpan;
                })
                .AddPolicyHandler(GetRetryPolicy())
                .AddPolicyHandler(GetCircuitBreakerPolicy())
                .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(90)));
        }

        public static void InitializePolly(IServiceProvider serviceProvider)
        {
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("Polly");
            ConfigureLogger(logger);
        }

        public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: attempt => TimeSpan.FromSeconds(10),
                    onRetry: (result, span, retry, _) =>
                    {
                        _logger?.LogWarning($"❗ Retry {retry} failed. Next in {span.TotalSeconds}s…");
                    });
        }


        public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .CircuitBreakerAsync(3, TimeSpan.FromSeconds(30),
                    onBreak: (result, timespan) =>
                    {
                        _logger?.LogError($"🔴 Circuit broken! Waiting {timespan.TotalSeconds} seconds...");
                    },
                    onReset: () =>
                    {
                        _logger?.LogInformation("🟢 Circuit reset. Resuming requests...");
                    });
        }
    }
}
