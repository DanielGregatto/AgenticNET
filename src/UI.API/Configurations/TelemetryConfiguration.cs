using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
            var instrumentationKey = configuration["ApplicationInsights:InstrumentationKey"];
            var connectionString = configuration["ApplicationInsights:ConnectionString"];

            if (!string.IsNullOrEmpty(instrumentationKey) &&
               !string.IsNullOrEmpty(connectionString))
            {
                services.AddApplicationInsightsTelemetry(options =>
                {
                    options.InstrumentationKey = instrumentationKey;
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
            var instrumentationKey = configuration["ApplicationInsights:InstrumentationKey"];
            var connectionString = configuration["ApplicationInsights:ConnectionString"];

            // Clear default providers
            logging.ClearProviders();

            // Add console logging with correlation ID support
            logging.AddSimpleConsole(options =>
            {
                options.IncludeScopes = true;   // Include correlation ID from scopes
                options.SingleLine = true;      // Better formatting for correlation ID
            });

            // Add debug output (local development)
            logging.AddDebug();

            // Add Application Insights logging provider (if configured)
            if (!string.IsNullOrEmpty(instrumentationKey) &&
               !string.IsNullOrEmpty(connectionString))
            {
                logging.AddApplicationInsights();

                // Set log level for non-production environments
                if (!environment.IsProduction())
                {
                    logging.AddFilter<Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider>(
                        "", LogLevel.Information);
                }
            }

            return logging;
        }
    }
}
