using Microsoft.AspNetCore.DataProtection;

namespace UI.API.Configurations
{
    public static class AzureStorageConfiguration
    {
        public static void AddAzureBlobDataProtection(this IServiceCollection services, IConfiguration configuration)
        {
            var appName = configuration["Application:Name"];

            services.AddDataProtection()
                    .SetApplicationName(appName);
        }
    }
}
