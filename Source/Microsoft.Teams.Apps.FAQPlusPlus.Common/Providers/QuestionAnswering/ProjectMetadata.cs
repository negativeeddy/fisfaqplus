// <copyright file="QnaServiceProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers.QuestionAnswering
{
    public class ProjectMetadata
    {
        public string createdDateTime { get; set; }
        public string description { get; set; }
        public string language { get; set; }
        public string lastDeployedDateTime { get; set; }
        public string lastModifiedDateTime { get; set; }
        public bool multilingualResource { get; set; }
        public string projectName { get; set; }
        public ProjectSettings settings { get; set; }
    }

    public class ProjectSettings
    {
        public string defaultAnswer { get; set; }
    }
}