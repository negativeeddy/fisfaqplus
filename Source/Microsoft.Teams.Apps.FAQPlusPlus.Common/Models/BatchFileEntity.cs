// <copyright file="TicketEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Microsoft.Azure.Search;
    using Microsoft.WindowsAzure.Storage.Table;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a BatchFile entity used for storage and retrieval.
    /// </summary>
    public class BatchFileEntity : TableEntity
    {
        /// <summary>
        /// Gets or sets the unique file id.
        /// </summary>
        [Key]
        [JsonProperty("Id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets contents of the batch file
        /// </summary>
        [JsonProperty("Status")]
        public byte[] FileBytes { get; set; }
    }
}