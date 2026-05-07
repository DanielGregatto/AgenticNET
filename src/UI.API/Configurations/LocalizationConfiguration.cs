using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

namespace UI.API.Configurations
{
    /// <summary>
    /// Extension methods for configuring multi-language support (localization)
    /// </summary>
    public static class LocalizationConfiguration
    {
        /// <summary>
        /// Adds localization services with support for English (en-US) and Portuguese (pt-BR)
        /// Language is selected based on the Accept-Language HTTP header
        /// </summary>
        public static IServiceCollection AddLocalizationConfiguration(this IServiceCollection services)
        {
            services.AddLocalization();

            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[]
                {
                    new CultureInfo("en-US"),   // English (default)
                    new CultureInfo("pt-BR")    // Portuguese (Brazil)
                };

                options.DefaultRequestCulture = new RequestCulture("en-US");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });

            return services;
        }
    }
}
