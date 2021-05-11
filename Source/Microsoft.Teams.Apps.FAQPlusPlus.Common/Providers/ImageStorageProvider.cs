using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers
{
    /// <summary>
    /// Used to store uploaded images to Blob Storage
    /// </summary>
    public class ImageStorageProvider : IImageStorageProvider
    {
        private readonly string storageConnectionString;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageStorageProvider"/> class.
        /// </summary>
        /// <param name="storageConnectionString"></param>
        public ImageStorageProvider(string storageConnectionString)
        {
            this.storageConnectionString = storageConnectionString;
        }

        /// <summary>
        /// Upload image and save to storage
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public async Task<string> UploadAsync(Stream stream, string fileName)
        {
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(this.storageConnectionString);

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve a reference to a container.
            // Container: faqplus-image-container (change in Constants if desired)
            CloudBlobContainer container = blobClient.GetContainerReference(Constants.ImageStorageContainer);

            // Create the container if it doesn't already exist.
            await container.CreateIfNotExistsAsync().ConfigureAwait(false);

            CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileName);

            // Upload the file
            await blockBlob.UploadFromStreamAsync(stream);

            return blockBlob.Uri.ToString();
        }


    }
}
