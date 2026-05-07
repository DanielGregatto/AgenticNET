using Identity.Authorization;
using Identity.Context;
using Identity.Model;
using Identity.Services;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Identity
{
    /// <summary>
    /// Extension methods for configuring ASP.NET Core Identity, JWT authentication, and social logins.
    /// Provides a complete authentication and authorization setup for the application.
    /// </summary>
    public static class IdentityConfig
    {
        /// <summary>
        /// Configures ASP.NET Core Identity with JWT bearer authentication and social login providers.
        /// Includes support for Google and Facebook authentication.
        /// </summary>
        /// <param name="services">The service collection to add Identity services to</param>
        /// <param name="configuration">Application configuration containing JWT and social login settings</param>
        public static void AddIdentityConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            // ============================================================================
            // JWT CONFIGURATION
            // ============================================================================
            // Load JWT configuration from appsettings
            var jwtConfig = new JWTConfig();
            configuration.GetSection("Jwt").Bind(jwtConfig);
            services.Configure<JWTConfig>(configuration.GetSection("Jwt"));

            // Register JWT-related services
            services.AddScoped<ISigningCredentialsConfiguration, SigningCredentialsConfiguration>();
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();

            // ============================================================================
            // IDENTITY DATABASE CONTEXT
            // ============================================================================
            // Configure separate DbContext for Identity tables (connection string configured in AppIdentityDbContext.OnConfiguring)
            services.AddDbContext<AppIdentityDbContext>();

            // ============================================================================
            // ASP.NET CORE IDENTITY
            // ============================================================================
            // Configure Identity with email confirmation requirement
            services
                .AddIdentityApiEndpoints<ApplicationUser>(options =>
                {
                    // Require email confirmation before allowing sign-in
                    options.SignIn.RequireConfirmedEmail = true;
                })
                .AddRoles<IdentityRole>()                           // Enable role-based authorization
                .AddEntityFrameworkStores<AppIdentityDbContext>()   // Use EF Core for Identity storage
                .AddDefaultTokenProviders();                        // Add token providers for password reset, email confirmation, etc.

            // ============================================================================
            // AUTHENTICATION CONFIGURATION
            // ============================================================================
            // Configure authentication schemes: JWT Bearer (API) + Cookie (Social logins)
            var key = Encoding.UTF8.GetBytes(jwtConfig.Secret);
            services.AddAuthentication(options =>
            {
                // Default scheme for API authentication
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

                // Cookie scheme for social login callbacks
                options.DefaultScheme = "ExternalScheme";
            })

            // ============================================================================
            // JWT BEARER AUTHENTICATION
            // ============================================================================
            // Configure JWT Bearer token validation
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                // Build a temporary service provider to access ISigningCredentialsConfiguration
                using (var scope = services.BuildServiceProvider().CreateScope())
                {
                    var signingCredentialsConfig = scope.ServiceProvider.GetRequiredService<ISigningCredentialsConfiguration>();

                    options.RequireHttpsMetadata = false;  // Allow HTTP in development (change for production)
                    options.SaveToken = true;               // Save token in AuthenticationProperties

                    // Configure token validation parameters
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = signingCredentialsConfig.Key,

                        ValidateIssuer = true,
                        ValidIssuer = jwtConfig.Issuer,

                        ValidateAudience = true,
                        ValidAudience = jwtConfig.Audience,

                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero  // No tolerance for token expiration
                    };
                }
            })

            // ============================================================================
            // COOKIE AUTHENTICATION (for social logins)
            // ============================================================================
            // Add cookie authentication scheme for external login providers
            .AddCookie("ExternalScheme")

            // ============================================================================
            // GOOGLE AUTHENTICATION
            // ============================================================================
            // Configure Google OAuth authentication
            .AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
            {
                options.ClientId = configuration["Authentication:Google:ClientId"];
                options.ClientSecret = configuration["Authentication:Google:ClientSecret"];
                options.SignInScheme = "ExternalScheme";  // Use cookie scheme for callback
            })

            // ============================================================================
            // FACEBOOK AUTHENTICATION
            // ============================================================================
            // Configure Facebook OAuth authentication
            .AddFacebook(FacebookDefaults.AuthenticationScheme, options =>
            {
                options.ClientId = configuration["Authentication:Facebook:AppId"];
                options.ClientSecret = configuration["Authentication:Facebook:AppSecret"];
                options.SignInScheme = "ExternalScheme";  // Use cookie scheme for callback
            });
        }
    }
}
