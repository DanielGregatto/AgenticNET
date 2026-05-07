using Azure.Storage.Blobs;
using Microsoft.AspNetCore.DataProtection.Repositories;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Services.Infrastructure
{
    /// <summary>
    /// Provides an <see cref="IXmlRepository"/> implementation that stores XML elements in an Azure Blob Storage
    /// object.
    /// </summary>
    /// <remarks>This repository enables the storage and retrieval of XML elements using a single Azure Blob
    /// as the backing store. It is typically used for persisting XML data such as cryptographic keys or configuration
    /// information in distributed or cloud-based applications.</remarks>
    public class AzureBlobXmlRepository : IXmlRepository
    {
        private readonly BlobClient _blobClient;

        public AzureBlobXmlRepository(BlobClient blobClient)
        {
            _blobClient = blobClient;
        }

        public IReadOnlyCollection<XElement> GetAllElements()
        {
            if (!_blobClient.Exists())
                return new List<XElement>();

            using var stream = _blobClient.OpenRead();
            var doc = XDocument.Load(stream);
            return doc.Root?.Elements().ToList().AsReadOnly() ?? new List<XElement>().AsReadOnly();
        }

        public void StoreElement(XElement element, string friendlyName)
        {
            var doc = new XDocument(new XElement("root", element));
            using var stream = new MemoryStream();
            doc.Save(stream);
            stream.Position = 0;
            _blobClient.Upload(stream, overwrite: true);
        }
    }

}