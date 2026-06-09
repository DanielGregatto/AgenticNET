namespace UI.API.Configurations
{
    /// <summary>
    /// Extension methods for configuring Application Insights telemetry and logging
    /// </summary>
    public static class TelemetryConfiguration
    {
        /// <summary>
        /// Adds Application Insights telemetry services if configured in appsettings
        /// </summary>
        public static IServiceCollection AddApplicationInsightsConfiguration(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString = configuration["ApplicationInsights:ConnectionString"];

            if (!string.IsNullOrEmpty(connectionString) && connectionString != "<env-var-terraform-set>")
            {
                services.AddApplicationInsightsTelemetry(options =>
                {
                    options.ConnectionString = connectionString;
                });
            }

            return services;
        }

        /// <summary>
        /// Configures logging providers including console, debug, and Application Insights
        /// </summary>
        public static ILoggingBuilder AddLoggingConfiguration(
            this ILoggingBuilder logging,
            IConfiguration configuration,
            IHostEnvironment environment)
        {
            // Clear default providers (does not affect AI — registered via DI, not the builder)
            logging.ClearProviders();

            // Console logging with scopes enabled so correlation IDs appear on every log line
            logging.AddSimpleConsole(options =>
            {
                options.IncludeScopes = true;
                options.SingleLine = true;
            });

            // Debug output for local development only
            logging.AddDebug();

            return logging;
        }
    }
}
