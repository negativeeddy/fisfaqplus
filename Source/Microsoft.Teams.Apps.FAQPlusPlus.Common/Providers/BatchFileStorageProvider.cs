// <copyright file="BatchFileStorageProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Exceptions;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// BatchFileStorage provider helps in fetching and storing information in storage table.
    /// </summary>
    public class BatchFileStorageProvider : IBatchFileProvider
    {
        private const string PartitionKey = "BatchFiles";
        private readonly Lazy<Task> initializeTask;
        private CloudTable batchFileCloudTable;

        /// <summary>
        /// Initializes a new instance of the <see cref="BatchFileStorageProvider"/> class.
        /// </summary>
        /// <param name="connectionString">connection string of storage provided by dependency injection.</param>
        public BatchFileStorageProvider(string connectionString)
        {
            this.initializeTask = new Lazy<Task>(() => this.InitializeTableStorageAsync(connectionString));
        }

        /// <summary>
        /// Store or update BatchFile entity in table storage.
        /// </summary>
        /// <param name="batchFile">Represents batchFile entity used for storage and retrieval.</param>
        /// <returns><see cref="Task"/> that represents configuration entity is saved or updated.</returns>
        public Task UpsertBatchFileAsync(BatchFileEntity batchFile)
        {
            batchFile.PartitionKey = PartitionKey;
            batchFile.RowKey = batchFile.Id;

            return this.StoreOrUpdateBatchFileEntityAsync(batchFile);
        }

        /// <summary>
        /// Get already saved entity detail from storage table.
        /// </summary>
        /// <param name="id">batchFile id received from bot based on which appropriate row data will be fetched.</param>
        /// <returns><see cref="Task"/> Already saved entity detail.</returns>
        public async Task<BatchFileEntity> GetBatchFileAsync(string id)
        {
            await this.EnsureInitializedAsync().ConfigureAwait(false); // When there is no batchFile created by end user
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            var searchOperation = TableOperation.Retrieve<BatchFileEntity>(PartitionKey, id);
            var searchResult = await this.batchFileCloudTable.ExecuteAsync(searchOperation).ConfigureAwait(false);

            return (BatchFileEntity)searchResult.Result;
        }

        /// <summary>
        /// Initialization of InitializeAsync method which will help in creating table.
        /// </summary>
        /// <returns>Represent a task with initialized connection data.</returns>
        private async Task EnsureInitializedAsync()
        {
            await this.initializeTask.Value.ConfigureAwait(false);
        }

        /// <summary>
        /// Create batchFiles table if it doesn't exist.
        /// </summary>
        /// <param name="connectionString">storage account connection string.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation task which represents table is created if its not existing.</returns>
        private async Task InitializeTableStorageAsync(string connectionString)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudTableClient cloudTableClient = storageAccount.CreateCloudTableClient();
            this.batchFileCloudTable = cloudTableClient.GetTableReference(Constants.BatchFileTableName);

            await this.batchFileCloudTable.CreateIfNotExistsAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Store or update batchFile entity in table storage.
        /// </summary>
        /// <param name="entity">Represents batchFile entity used for storage and retrieval.</param>
        /// <returns><see cref="Task"/> that represents configuration entity is saved or updated.</returns>
        private async Task<TableResult> StoreOrUpdateBatchFileEntityAsync(BatchFileEntity entity)
        {
            await this.EnsureInitializedAsync().ConfigureAwait(false);
            TableOperation addOrUpdateOperation = TableOperation.InsertOrReplace(entity);
            return await this.batchFileCloudTable.ExecuteAsync(addOrUpdateOperation).ConfigureAwait(false);
        }
    }
}
