// <copyright file="BotSettings.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Models.Configuration
{
    /// <summary>
   /// Provides app settings related to FaqPlusPlus bot.
   /// </summary>
    public class BotSettings
    {
        /// <summary>
        /// Gets or sets access cache expiry in days.
        /// </summary>
        public int AccessCacheExpiryInDays { get; set; }

        /// <summary>
        /// Gets or sets application base uri.
        /// </summary>
        public string AppBaseUri { get; set; }

        /// <summary>
        /// Gets or sets microsoft app id.
        /// </summary>
        public string MicrosoftAppId { get; set; }

        /// <summary>
        /// Gets or sets access tenant id string.
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// Gets or sets the Azure storage connection string
        /// </summary>
        public string StorageConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the value indicating whether to use the normal
        /// upload process for bulk QnA answers or to store them in blob storage
        /// </summary>
        public bool UseBlobStorageForBulkUploadResults = false;

    }
}