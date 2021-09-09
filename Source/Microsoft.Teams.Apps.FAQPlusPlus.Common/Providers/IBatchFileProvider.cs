// <copyright file="IBatchFileProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers
{
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;

    /// <summary>
    /// Interface of BatchFiles provider.
    /// </summary>
    public interface IBatchFileProvider
    {
        /// <summary>
        /// Save or update batch file data
        /// </summary>
        /// <param name="batchFile">A file containing batch processed FAQ answers</param>
        /// <returns><see cref="Task"/> that resolves successfully if the data was saved successfully.</returns>
        Task UpsertBatchFileAsync(BatchFileEntity batchFile);

        /// <summary>
        /// Get already saved entity detail from storage table.
        /// </summary>
        /// <param name="id">BatchFile id received from bot based on which appropriate row data will be fetched.</param>
        /// <returns><see cref="Task"/> Already saved entity detail.</returns>
        Task<BatchFileEntity> GetBatchFileAsync(string id);
    }
}
