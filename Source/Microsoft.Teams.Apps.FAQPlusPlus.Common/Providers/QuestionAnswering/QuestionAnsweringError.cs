// <copyright file="QnaServiceProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers.QuestionAnswering
{
    using Microsoft.Azure.CognitiveServices.Knowledge.QnAMaker.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class QuestionAnsweringError
    {
        public string code { get; set; }

        public string message { get; set; }

        public string target { get; set; }

        public Dictionary<string, string> details { get; set; }
        public QuestionAnsweringError innerError { get; set; }
    }
}