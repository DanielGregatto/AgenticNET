using Azure.Identity;
using Domain.Configs;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using System;

namespace UI.API.Configurations
{
    public static class AzureStorageConfiguration
    {
        public static void AddAzureBlobDataProtection(this IServiceCollection services, IConfiguration configuration)
        {
            var appName = configuration["Application:Name"];
            var storageConfig = configuration.GetSection("AzureStorage").Get<AzureStorageConfig>();

            var builder = services.AddDataProtection().SetApplicationName(appName);

            if (storageConfig is not null && !string.IsNullOrWhiteSpace(storageConfig.AccountName))
            {
                var blobUri = new Uri(
                    $"https://{storageConfig.AccountName}.blob.core.windows.net" +
                    $"/{storageConfig.Container}/{storageConfig.BlobName}");

                builder.PersistKeysToAzureBlobStorage(blobUri, new DefaultAzureCredential());
            }
        }
    }
}
