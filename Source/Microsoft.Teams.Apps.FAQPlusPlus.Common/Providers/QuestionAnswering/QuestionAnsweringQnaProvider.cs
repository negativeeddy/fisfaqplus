// <copyright file="QnaServiceProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers.QuestionAnswering
{
    using global::Azure;
    using global::Azure.AI.Language.QuestionAnswering;
    using global::Azure.AI.Language.QuestionAnswering.Projects;
    using global::Azure.Core;
    using Microsoft.Azure.CognitiveServices.Knowledge.QnAMaker;
    using Microsoft.Azure.CognitiveServices.Knowledge.QnAMaker.Models;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models.Configuration;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;

    /// <summary>
    /// Qna maker service provider class.
    /// </summary>
    public partial class QuestionAnsweringQnaProvider : IQnaServiceProvider
    {
        /// <summary>
        /// Environment type.
        /// </summary>
        private QuestionAnsweringClient _answerClient = null;
        private QuestionAnsweringProjectsClient _projectClient = null;
        private readonly string _questionAnsweringEndpoint;
        private readonly string _questionAnsweringKey;
        private string _projectName = null;
        private const string _testDeploymentName = "test";
        private const string _productionDeploymentName = "production";

        private readonly IConfigurationDataProvider configurationProvider;

        /// <summary>
        /// Represents a set of key/value application configuration properties.
        /// </summary>
        private readonly QnAMakerSettings options;

        /// <summary>
        /// Initializes a new instance of the <see cref="QnaServiceProvider"/> class.
        /// </summary>
        /// <param name="configurationProvider">ConfigurationProvider fetch and store information in storage table.</param>
        /// <param name="optionsAccessor">A set of key/value application configuration properties.</param>
        /// <param name="qnaMakerClient">Qna service client.</param>
        /// <param name="qnaMakerRuntimeClient">Qna service runtime client.</param>
        public QuestionAnsweringQnaProvider(IConfiguration config, IConfigurationDataProvider configurationProvider, IOptionsMonitor<QnAMakerSettings> optionsAccessor)
        {
            this.configurationProvider = configurationProvider;
            options = optionsAccessor.CurrentValue;
            _questionAnsweringEndpoint = config["QnAMakerApiEndpointUrl"];
            _questionAnsweringKey = config["QnAMakerSubscriptionKey"];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QnaServiceProvider"/> class.
        /// </summary>
        /// <param name="configurationProvider">ConfigurationProvider fetch and store information in storage table.</param>
        /// <param name="optionsAccessor">A set of key/value application configuration properties.</param>
        /// <param name="qnaMakerClient">Qna service client.</param>
        public QuestionAnsweringQnaProvider(IConfiguration config, IConfigurationDataProvider configurationProvider, IOptionsMonitor<QnAMakerSettings> optionsAccessor, IQnAMakerClient qnaMakerClient)
        {
            this.configurationProvider = configurationProvider;
            options = optionsAccessor.CurrentValue;
            _questionAnsweringEndpoint = config["QnAMakerApiEndpointUrl"];
            _questionAnsweringKey = config["QnAMakerSubscriptionKey"];
        }

        private async Task<string> GetProjectNameAsync()
        {
            if (_projectName == null)
            {
                _projectName = await this.configurationProvider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.KnowledgeBaseId).ConfigureAwait(false);
            }

            return _projectName;
        }

        private async Task<QuestionAnsweringProjectsClient> GetAnsweringProjectClient()
        {
            if (_projectClient == null)
            {
                Uri endpoint = new Uri(_questionAnsweringEndpoint);
                AzureKeyCredential credential = new AzureKeyCredential(_questionAnsweringKey);
                _projectClient = new QuestionAnsweringProjectsClient(endpoint, credential);
            }

            return _projectClient;
        }

        private async Task<QuestionAnsweringClient> GetAnsweringClient()
        {
            if (_answerClient == null)
            {
                Uri endpoint = new Uri(_questionAnsweringEndpoint);
                AzureKeyCredential credential = new AzureKeyCredential(_questionAnsweringKey);
                _answerClient = new QuestionAnsweringClient(endpoint, credential);
            }

            return _answerClient;
        }

        /// <summary>
        /// This method is used to add QnA pair in Kb.
        /// </summary>
        /// <param name="question">Question text.</param>
        /// <param name="combinedDescription">Answer text.</param>
        /// <param name="createdBy">Created by user.</param>
        /// <param name="conversationId">Conversation id.</param>
        /// <param name="activityReferenceId">Activity reference id refer to activityid in storage table.</param>
        /// <returns>Operation state as task.</returns>
        public async Task AddQnaAsync(string question, string combinedDescription, string createdBy, string conversationId, string activityReferenceId)
        {
            var client = await GetAnsweringProjectClient();

            question = question.Trim();
            string answer = combinedDescription.Trim();
            RequestContent updateQnasRequestContent = RequestContent.Create(
                new UpdateQnaRecord[] {
                    new UpdateQnaRecord {
                        op = UpdateQnaRecord.OpAdd,
                        value = new QnaRecord
                        {
                            questions = new[]
                                {
                                    question,
                                },
                            answer = answer,
                            metadata = new Dictionary<string, string>
                            {
                                {Constants.MetadataCreatedAt, DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture) },
                                {Constants.MetadataCreatedBy, createdBy },
                                {Constants.MetadataConversationId, HttpUtility.UrlEncode(conversationId) },
                                {Constants.MetadataActivityReferenceId, activityReferenceId },
                            },

                        },
                    },
                });

            string projectName = await GetProjectNameAsync();
            Operation<BinaryData> updateQnasOperation = await client.UpdateQnasAsync(false, projectName, updateQnasRequestContent);
            string data = (await updateQnasOperation.WaitForCompletionAsync()).ToString();
        }

        /// <summary>
        /// Update Qna pair in knowledge base.
        /// </summary>
        /// <param name="questionId">Question id.</param>
        /// <param name="answer">Answer text.</param>
        /// <param name="updatedBy">Updated by user.</param>
        /// <param name="updatedQuestion">Updated question text.</param>
        /// <param name="question">Original question text.</param>
        /// <returns>Perfomed action task.</returns>
        public async Task UpdateQnaAsync(int questionId, string answer, string updatedBy, string updatedQuestion, string question, string conversationId = null, string activityReferenceId = null)
        {
            var client = await GetAnsweringProjectClient();

            var questions = default(UpdateQnaDTOQuestions);
            if (!string.IsNullOrEmpty(updatedQuestion?.Trim()))
            {
                questions = updatedQuestion?.ToUpperInvariant().Trim() == question?.ToUpperInvariant().Trim() ? null
                    : new UpdateQnaDTOQuestions()
                    {
                        Add = new List<string> { updatedQuestion.Trim() },
                        Delete = new List<string> { question.Trim() },
                    };
            }

            answer = answer.Trim();

            var metadataDTOs = new Dictionary<string, string>()
            {
                { Constants.MetadataUpdatedBy,  updatedBy},
                { Constants.MetadataUpdatedAt,DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture) },
            };

            if (activityReferenceId != null)
            {
                metadataDTOs.Add(Constants.MetadataActivityReferenceId, activityReferenceId);
            }

            if (conversationId != null)
            {
                metadataDTOs.Add(Constants.MetadataConversationId, HttpUtility.UrlEncode(conversationId));
                metadataDTOs.Add(Constants.MetadataCreatedAt, DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture));
            }

            RequestContent updateQnasRequestContent = RequestContent.Create(
                new UpdateQnaRecord[] {
                    new UpdateQnaRecord {
                        op = "replace",
                        value = new QnaRecord
                        {
                            id = questionId,
                            source = Constants.Source,
                            questions = new[]
                                {
                                    question,
                                },
                            answer = answer,
                            metadata = metadataDTOs,
                        },
                    },
                });

            string projectName = await GetProjectNameAsync();

            Operation<BinaryData> updateQnasOperation = await client.UpdateQnasAsync(false, projectName, updateQnasRequestContent);
            string data = (await updateQnasOperation.WaitForCompletionAsync()).ToString();
        }

        /// <summary>
        /// This method is used to delete Qna pair from KB.
        /// </summary>
        /// <param name="questionId">Question id.</param>
        /// <returns>Perfomed action task.</returns>
        public async Task DeleteQnaAsync(int questionId)
        {
            var client = await GetAnsweringProjectClient();

            RequestContent deleteContent = RequestContent.Create(
                new UpdateQnaRecord[]
                {
                    new UpdateQnaRecord
                    {
                        op = UpdateQnaRecord.OpDelete,
                        value = new QnaRecord
                        {
                            id = questionId,
                        },
                    },
                });

            string projectName = await GetProjectNameAsync();
            Operation<BinaryData> updateQnasOperation = await client.UpdateQnasAsync(false, projectName, deleteContent);
            string data = (await updateQnasOperation.WaitForCompletionAsync()).ToString();
        }

        /// <summary>
        /// Get answer from knowledgebase for a given question.
        /// </summary>
        /// <param name="question">Question text.</param>
        /// <param name="isTestKnowledgeBase">Prod or test.</param>
        /// <param name="previousQnAId">Id of previous question.</param>
        /// <param name="previousUserQuery">Previous question information.</param>
        /// <returns>QnaSearchResultList result as response.</returns>
        public async Task<QnASearchResultList> GenerateAnswerAsync(string question, bool isTestKnowledgeBase, string previousQnAId = null, string previousUserQuery = null, IList<QueryTag> tags = null)
        {
            var client = await GetAnsweringClient();

            double threshold = Convert.ToDouble(this.options.ScoreThreshold, CultureInfo.InvariantCulture);

            QueryFilters filter = null;
            if (tags != null)
            {
                filter = new QueryFilters()
                {
                    MetadataFilter = new MetadataFilter()
                    {
                        LogicalOperation = LogicalOperationKind.And,
                    },
                };

                foreach (var tag in tags)
                {
                    filter.MetadataFilter.Metadata.Add(new MetadataRecord(tag.Name, tag.Value));
                }
            }

            string projectName = await GetProjectNameAsync();
            string deployment = isTestKnowledgeBase ? _testDeploymentName : _productionDeploymentName;
            QuestionAnsweringProject project = new QuestionAnsweringProject(projectName, deployment);
            Response<AnswersResult> response = client.GetAnswers(
                question,
                project,
                new AnswersOptions()
                {
                    ConfidenceThreshold = threshold,
                    Filters = filter,
                });

            // convert AnswerResult to old QnASearchResultList
            var results = response.Value.Answers.Select(a => new QnASearchResult(
                                                                questions: a.Questions.ToList(),
                                                                metadata: a.Metadata.Select(m =>
                                                                                new MetadataDTO(name: m.Key, value: m.Value)).ToList(),
                                                                answer: a.Answer,
                                                                id: a.QnaId,
                                                                score: a.Confidence,
                                                                context: ConvertToQnaContext(a.Dialog)
                                                                )
                        );
            var result = new QnASearchResultList(results.ToList());
            return result;
        }

        private QnASearchResultContext ConvertToQnaContext(KnowledgeBaseAnswerDialog dialog)
        {
            if (dialog == null)
            {
                return null;
            }

            return new QnASearchResultContext(isContextOnly: dialog.IsContextOnly,
                prompts: dialog.Prompts.Select(
                    p => new PromptDTO(
                        displayOrder: p.DisplayOrder,
                        qnaId: p.QnaId,
                        displayText:
                        p.DisplayText)).ToList());
        }

        /// <summary>
        /// This method returns the downloaded knowledgebase documents.
        /// </summary>
        /// <param name="knowledgeBaseId">Knowledgebase Id.</param>
        /// <returns>List of question and answer document object.</returns>
        public async Task<IEnumerable<QnADTO>> DownloadKnowledgebaseAsync(string knowledgeBaseId, bool isTestEnvironment = false)
        {
            List<QnADTO> qnas = new List<QnADTO>();

            var client = await GetAnsweringProjectClient();

            string projectName = await GetProjectNameAsync();
            AsyncPageable<BinaryData> response = client.GetQnasAsync(projectName);

            // cant use async iterators in standard 2.0, do it manually
            var enumerator = response.GetAsyncEnumerator();
            while (await enumerator.MoveNextAsync())
            {
                var item = enumerator.Current;
                QnaRecord page = item.ToObjectFromJson<QnaRecord>();
                if (page != null)
                {
                    qnas.Add(page.ToQnaDto());
                    continue;
                }

                ErrorResponse error = item.ToObjectFromJson<ErrorResponse>();
                if (error != null)
                {
                    throw new InvalidOperationException(error.error.message);
                }
            }

            return qnas;
        }

        /// <summary>
        /// Checks whether knowledgebase need to be published.
        /// </summary>
        /// <param name="knowledgeBaseId">Knowledgebase id.</param>
        /// <returns>A <see cref="Task"/> of type bool where true represents knowledgebase need to be published while false indicates knowledgebase not need to be published.</returns>
        public async Task<bool> GetPublishStatusAsync(string projectName)
        {
            var client = await GetAnsweringProjectClient();

            var response = await client.GetProjectDetailsAsync(projectName);
            if (response.Status == 200)
            {
                var project = response.Content.ToObjectFromJson<ProjectMetadata>();
                if (project != null && project.lastModifiedDateTime != null && project.lastDeployedDateTime != null)
                {
                    return Convert.ToDateTime(project.lastModifiedDateTime) > Convert.ToDateTime(project.lastDeployedDateTime);
                }
            }

            return true;
        }

        /// <summary>
        /// Method is used to publish knowledgebase.
        /// </summary>
        /// <param name="knowledgeBaseId">Knowledgebase Id.</param>
        /// <returns>Task for published data.</returns>
        public async Task PublishKnowledgebaseAsync(string projectName)
        {
            var client = await GetAnsweringProjectClient();

            var response = client.DeployProjectAsync(false, projectName, _productionDeploymentName);
        }

        /// <summary>
        /// Get knowledgebase published information.
        /// </summary>
        /// <param name="knowledgeBaseId">Knowledgebase id.</param>
        /// <returns>A <see cref="Task"/> of type bool where true represents knowledgebase has published atleast once while false indicates that knowledgebase has not published yet.</returns>
        public async Task<bool> GetInitialPublishedStatusAsync(string projectName)
        {
            var client = await GetAnsweringProjectClient();

            var response = await client.GetProjectDetailsAsync(projectName);
            if (response.Status == 200)
            {
                var project = response.Content.ToObjectFromJson<ProjectMetadata>();
                return !string.IsNullOrEmpty(project.lastDeployedDateTime);
            }

            return false;
        }
    }
}
