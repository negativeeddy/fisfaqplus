// <copyright file="QnaServiceProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers.QuestionAnswering
{
    using Microsoft.Azure.CognitiveServices.Knowledge.QnAMaker.Models;
    using System.Collections.Generic;
    using System.Linq;

    public class QnaRecord
    {
        public string answer { get; set; }
        public int id { get; set; }
        public string[] questions { get; set; }
        public string source { get; set; }
        public Dictionary<string, string> metadata { get; set; }

        public QnADTO ToQnaDto()
        {
            return new QnADTO
            {
                Answer = answer,
                Id = id,
                Questions = questions,
                Source = source,
                Metadata = metadata.Select((item) => new MetadataDTO(item.Key, item.Value)).ToArray(),
            };
        }
    }
}