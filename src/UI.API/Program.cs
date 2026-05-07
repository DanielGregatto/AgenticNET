using Identity;
using IoC;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;
using UI.API.Configurations;
using UI.API.Middleware;

namespace UI.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ============================================================================
            // CONFIGURATION SOURCES
            // ============================================================================
            // Load configuration from multiple sources in priority order:
            // 1. appsettings.json (base configuration)
            // 2. appsettings.{Environment}.json (environment-specific overrides)
            // 3. Environment variables (deployment overrides)
            // 4. User secrets (development only - for sensitive data)
            builder.Configuration
                   .SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                   .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
                   .AddEnvironmentVariables();

            if (builder.Environment.IsDevelopment())
                builder.Configuration.AddUserSecrets<Program>();

            // ============================================================================
            // TELEMETRY & LOGGING
            // ============================================================================
            // Configure Application Insights telemetry and logging providers
            builder.Services.AddApplicationInsightsConfiguration(builder.Configuration);
            builder.Logging.AddLoggingConfiguration(builder.Configuration, builder.Environment);

            // ============================================================================
            // DEPENDENCY INJECTION
            // ============================================================================
            // Register database, services, repositories, validators, and MediatR handlers
            DIBootstrapper.RegisterCustomServices(builder.Services, builder.Configuration);

            // ============================================================================
            // LOCALIZATION
            // ============================================================================
            // Configure multi-language support (en-US, pt-BR)
            // Language is selected via Accept-Language HTTP header
            builder.Services.AddLocalizationConfiguration();

            // ============================================================================
            // CORS (Cross-Origin Resource Sharing)
            // ============================================================================
            // Configure CORS to allow frontend applications to access the API
            // Allowed origins are configured per environment in appsettings.json
            var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                ?? new[] { "http://localhost:4200" };

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend",
                    policy =>
                    {
                        policy.WithOrigins(allowedOrigins)
                              .AllowAnyMethod()
                              .AllowAnyHeader()
                              .AllowCredentials();
                    });
            });

            // ============================================================================
            // SECURITY & IDENTITY
            // ============================================================================
            // Configure ASP.NET Core Identity, JWT authentication, and social logins
            builder.Services.AddIdentityConfiguration(builder.Configuration);

            // Configure rate limiting to prevent API abuse
            builder.Services.AddRateLimiterConfiguration();

            // Configure Azure Blob Storage for DataProtection keys (distributed scenarios)
            builder.Services.AddAzureBlobDataProtection(builder.Configuration);

            // Configure forwarded headers for reverse proxy support (Azure, nginx, etc.)
            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedFor;
                options.KnownProxies.Clear();
                options.KnownNetworks.Clear();
            });

            // ============================================================================
            // API CONFIGURATION
            // ============================================================================
            // Configure AutoMapper for object-to-object mapping
            builder.Services.AddAutoMapperConfiguration();

            // Configure controllers with JSON serialization options
            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never;
            });

            // Configure API documentation (Swagger/OpenAPI)
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerConfig();

            // Configure Polly resilience policies (retry, circuit breaker, etc.)
            builder.Services.AddPollyResilience();

            // Configure global exception handling
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();

            // ============================================================================
            // BUILD APPLICATION
            // ============================================================================
            var app = builder.Build();

            // Initialize Polly policies
            PollyConfiguration.InitializePolly(app.Services);

            // ============================================================================
            // MIDDLEWARE PIPELINE
            // ============================================================================
            // Configure HTTP request pipeline (ORDER MATTERS!)
            // Middleware executes in the order it's added

            // 1. Forwarded headers (must be first for reverse proxy scenarios)
            app.UseForwardedHeaders();

            // 2. Localization (early in pipeline to set culture for entire request)
            var localizationOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value;
            app.UseRequestLocalization(localizationOptions);

            // 3. Correlation ID (for request tracing across services)
            app.UseMiddleware<CorrelationIdMiddleware>();

            // 4. Exception handling (catch unhandled exceptions)
            app.UseExceptionHandler(options => { });

            // 5. Swagger UI (development only)
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // 6. HTTPS redirection
            app.UseHttpsRedirection();

            // 7. CORS (must be before Authentication/Authorization)
            app.UseCors("AllowFrontend");

            // 8. Authentication (verify user identity)
            app.UseAuthentication();

            // 9. Authorization (verify user permissions)
            app.UseAuthorization();

            // 10. Rate limiting (prevent API abuse)
            app.UseRateLimiter();

            // 11. Map controllers (route requests to endpoints)
            app.MapControllers();

            // ============================================================================
            // START APPLICATION
            // ============================================================================
            app.Run();
        }
    }
}
