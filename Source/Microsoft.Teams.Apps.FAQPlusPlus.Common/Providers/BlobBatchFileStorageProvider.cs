// <copyright file="BatchFileStorageProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Exceptions;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Auth;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// BlobBatchFileStorageProvider provider helps in fetching and storing information in storage blobs.
    /// </summary>
    public class BlobBatchFileStorageProvider : IBatchFileProvider
    {
        private const string ContainerName = "BatchFiles";
        private readonly TelemetryClient telemetryClient;
        private readonly Lazy<Task> initializeTask;
        private CloudBlobContainer batchFileBlobContainer;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobBatchFileStorageProvider"/> class.
        /// </summary>
        /// <param name="connectionString">connection string of storage provided by dependency injection.</param>
        public BlobBatchFileStorageProvider(string connectionString, TelemetryClient telemetryClient)
        {
            this.telemetryClient = telemetryClient;
            this.initializeTask = new Lazy<Task>(() => this.InitializeBlobStorageAsync(connectionString));
        }

        /// <summary>
        /// Store or update BatchFile entity in blob storage.
        /// </summary>
        /// <param name="batchFile">Represents batchFile entity used for storage and retrieval.</param>
        /// <returns><see cref="Task"/> that represents configuration entity is saved or updated.</returns>
        public async Task UpsertBatchFileAsync(BatchFileEntity batchFile)
        {
            await this.EnsureInitializedAsync().ConfigureAwait(false); // When there is no batchFile created by end user
            await this.StoreOrUpdateBatchFileEntityAsync(batchFile);
        }

        /// <summary>
        /// Get already saved entity detail from blob table.
        /// </summary>
        /// <param name="id">batchFile id received from bot based on which appropriate row data will be fetched.</param>
        /// <returns><see cref="Task"/> Already saved entity detail.</returns>
        public async Task<BatchFileEntity> GetBatchFileAsync(string id)
        {
            try
            {
                telemetryClient.TrackEvent("FetchingBatchBlob", new Dictionary<string, string>
                {
                    { "name", id },
                });

                await this.EnsureInitializedAsync().ConfigureAwait(false); // When there is no batchFile created by end user
                if (string.IsNullOrEmpty(id))
                {
                    return null;
                }

                var blobClient = batchFileBlobContainer.GetBlockBlobReference(id);
                var stream = new MemoryStream();
                await blobClient.DownloadToStreamAsync(stream);
                stream.Position = 0;
                var result = new BatchFileEntity()
                {
                    Id = id,
                    FileBytes = stream.ToArray(),
                };

                telemetryClient.TrackEvent("FetchedBatchBlob", new Dictionary<string, string>
                {
                    { "name", id },
                });

                return result;
            }
            catch (Exception ex)
            {
                telemetryClient.TrackEvent("FailedFetchingBatchBlob", new Dictionary<string, string>
                {
                    { "name", id },
                });
                throw;
            }
        }

        /// <summary>
        /// Initialization of InitializeAsync method which will help in creating blob container.
        /// </summary>
        /// <returns>Represent a task with initialized connection data.</returns>
        private async Task EnsureInitializedAsync()
        {
            await this.initializeTask.Value.ConfigureAwait(false);
        }

        /// <summary>
        /// Create batchFiles blob if it doesn't exist.
        /// </summary>
        /// <param name="connectionString">storage account connection string.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation task which represents table is created if its not existing.</returns>
        private async Task InitializeBlobStorageAsync(string connectionString)
        {
            telemetryClient.TrackEvent("InitializingBatchBlobStorage");

            try
            {
                DbConnectionStringBuilder builder = new DbConnectionStringBuilder();
                builder.ConnectionString = connectionString;

                string uri = $"https://{builder["AccountName"]}.blob.{builder["EndpointSuffix"]}";

                // Create a BlobServiceClient object which will be used to create a container client
                CloudBlobClient blobClient = new CloudBlobClient(
                    new Uri(uri),
                    new StorageCredentials((string)builder["AccountName"], (string)builder["AccountKey"]));

                // Create the container and return a container client object
                batchFileBlobContainer = blobClient.GetContainerReference(Constants.BatchBlobName);
                await batchFileBlobContainer.CreateIfNotExistsAsync(BlobContainerPublicAccessType.Off, null, null);
                telemetryClient.TrackEvent("InitializedBatchBlobStorage");
            }
            catch (Exception ex)
            {
                telemetryClient.TrackEvent("FailedInitializingBatchBlobStorage");
                throw;
            }
        }

        /// <summary>
        /// Store or update batchFile entity in blob storage.
        /// </summary>
        /// <param name="entity">Represents batchFile entity used for storage and retrieval.</param>
        /// <returns><see cref="Task"/> that represents configuration entity is saved or updated.</returns>
        private async Task StoreOrUpdateBatchFileEntityAsync(BatchFileEntity entity)
        {
            telemetryClient.TrackEvent("StoringBatchBlob", new Dictionary<string, string>
            {
                { "name", entity.Id },
                { "bytes", entity.FileBytes.Length.ToString() },
            });

            try
            {
                // Get a reference to a blob
                var blobClient = batchFileBlobContainer.GetBlockBlobReference(entity.Id);
                await blobClient.UploadFromByteArrayAsync(entity.FileBytes, 0, entity.FileBytes.Length);

                telemetryClient.TrackEvent("StoredBatchBlob", new Dictionary<string, string>
                {
                    { "name", entity.Id },
                    { "uri", blobClient.Uri.ToString() },
                });
            }
            catch (Exception ex)
            {
                telemetryClient.TrackEvent("FailedStoringBatchBlob");
                throw;
            }
        }
    }
}
