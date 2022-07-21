// <copyright file="QnaServiceProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers.QuestionAnswering
{
    public class UpdateQnaRecord
    {
        public string op { get; set; }
        public QnaRecord @value { get; set; }

        public const string OpAdd = "add";
        public const string OpDelete = "delete";
        public const string OpReplace = "replace";
    }
}
