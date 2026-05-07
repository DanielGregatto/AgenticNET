using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.Extensions.Options;

namespace Services.Infrastructure
{
    /// <summary>
    /// Configures <see cref="KeyManagementOptions"/> to use an <see cref="IXmlRepository"/> for XML key storage,
    /// typically with Azure Blob Storage as the backing store.
    /// </summary>
    /// <remarks>This class is intended for use with dependency injection to set up data protection key
    /// management options that persist keys using an <see cref="IXmlRepository"/> implementation, such as one backed by
    /// Azure Blob Storage. It is commonly registered in the application's service configuration to enable distributed
    /// key storage across multiple instances.</remarks>
    public class AzureBlobKeyManagementOptionsSetup : IConfigureOptions<KeyManagementOptions>
    {
        private readonly IXmlRepository _xmlRepository;

        public AzureBlobKeyManagementOptionsSetup(IXmlRepository xmlRepository)
        {
            _xmlRepository = xmlRepository;
        }

        public void Configure(KeyManagementOptions options)
        {
            options.XmlRepository = _xmlRepository;
        }
    }
}
