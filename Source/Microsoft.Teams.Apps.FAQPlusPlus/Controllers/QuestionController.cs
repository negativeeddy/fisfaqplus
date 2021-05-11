namespace Microsoft.Teams.Apps.FAQPlusPlus.Controllers
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.CognitiveServices.Knowledge.QnAMaker.Models;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models.Configuration;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers;
    using Microsoft.Teams.Apps.FAQPlusPlus.Helpers;
    using Newtonsoft.Json;

    /// <summary>
    /// Controller to update QnA answers and upload images.
    /// </summary>
    [Route("/question")]
    public class QuestionController : Controller
    {
        private readonly IConfigurationDataProvider configurationProvider;
        private readonly IQnaServiceProvider qnaServiceProvider;
        private readonly IImageStorageProvider imageStorageProvider;
        private readonly ILogger<QuestionController> logger;
        private readonly BotSettings options;
        private readonly string appId;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionController"/> class.
        /// </summary>
        /// <param name="configurationProvider"></param>
        /// <param name="qnaServiceProvider"></param>
        /// <param name="imageStorageProvider"></param>
        /// <param name="optionsAccessor"></param>
        public QuestionController(IConfigurationDataProvider configurationProvider, IQnaServiceProvider qnaServiceProvider, IImageStorageProvider imageStorageProvider, IOptionsMonitor<BotSettings> optionsAccessor, ILogger<QuestionController> logger)
        {
            this.configurationProvider = configurationProvider;
            this.qnaServiceProvider = qnaServiceProvider;
            this.imageStorageProvider = imageStorageProvider;
            this.logger = logger;

            this.options = optionsAccessor.CurrentValue;
            this.appId = this.options.MicrosoftAppId;
        }


        // GET: QuestionController
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Get the QnA Pair for Editing
        /// </summary>
        /// <param name="id"></param>
        /// <param name="question"></param>
        /// <param name="answer"></param>
        /// <returns></returns>
        //// GET: QuestionController/Edit/5
        [Route("/question/edit/{id}")]
        public async Task<ActionResult> Edit(int id, string question, string answer)
        {
            var qnaModel = new QnAQuestionModel();
            AdaptiveSubmitActionData postedValues = new AdaptiveSubmitActionData();

            // if its an existing question, prepopulate the values from the kb
            if (id > 0)
            {
                QnADTO answerData = null;
                try
                {
                    var knowledgeBaseId = await this.configurationProvider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.KnowledgeBaseId).ConfigureAwait(false);
                    var qnaitems = await this.qnaServiceProvider.DownloadKnowledgebaseAsync(knowledgeBaseId, true);
                    answerData = qnaitems.FirstOrDefault(k => k.Id == id);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"failed to load qna id {id}");
                }

                if (answerData != null)
                {
                    postedValues.QnaPairId = id;
                    postedValues.OriginalQuestion = answerData.Questions[0];
                    postedValues.UpdatedQuestion = answerData.Questions[0];

                    if (Validators.IsValidJSON(answerData.Answer))
                    {
                        AnswerModel answerModel = JsonConvert.DeserializeObject<AnswerModel>(answerData.Answer);
                        postedValues.Description = answerModel.Description;
                        postedValues.Title = answerModel.Title;
                        postedValues.Subtitle = answerModel.Subtitle;
                        postedValues.ImageUrl = answerModel.ImageUrl;
                        postedValues.RedirectionUrl = answerModel.RedirectionUrl;
                    }
                    else
                    {
                        postedValues.Description = answerData.Answer;
                    }
                }
                else
                {
                    postedValues.Description = "ERROR: QnA Pair Not Found";
                }
            }

            qnaModel.PostedValues = postedValues;
            qnaModel.AppId = this.appId;

            return View(qnaModel);
        }

        /// <summary>
        /// Posting edited Answer
        /// </summary>
        /// <param name="id"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        // POST: QuestionController/Edit/5
        [Route("/question/edit/{id}")]
        [HttpPost]
        public async Task<ActionResult> Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        /// <summary>
        /// Upload Image To Blob Storage
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        [Route("/question/upload")]
        [HttpPost]
        public async Task<ActionResult> Upload(IFormCollection collection)
        {

            string url = String.Empty;
            string fileName = String.Empty;

            Console.WriteLine(collection.Count);
            if (collection.Files.Count > 0)
            {
                if (collection.Files[0] != null)
                {
                    var file = collection.Files[0];

                    if (IsImage(file))
                    {
                        if (file.Length > 0)
                        {
                            using (Stream stream = file.OpenReadStream())
                            {
                                // Get the reference to the block blob from the container
                                string orginalFileName = file.FileName;
                                if (file.FileName.LastIndexOf("\\") > -1)
                                {
                                    orginalFileName = file.FileName.Substring(file.FileName.LastIndexOf("\\") + 1,
                                    file.FileName.Length - file.FileName.LastIndexOf("\\") - 1);
                                }

                                string filenamePrefix = DateTime.Now.ToString("yyyyMMddHHmmss"); // Makes filename unique
                                url = await this.imageStorageProvider.UploadAsync(stream, $"{filenamePrefix}_{orginalFileName}");
                            }
                        }
                    }
                }
            }

            // CKEDitor requires image url to be passed in JSON
            return Json(new { Url = url });
        }

        /// <summary>
        /// Checks to see if image is one of allowable types
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private bool IsImage(IFormFile file)
        {
            if (file.ContentType.Contains("image"))
            {
                return true;
            }

            string[] formats = new string[] { ".jpg", ".png", ".gif", ".jpeg" };

            return formats.Any(item => file.FileName.EndsWith(item, StringComparison.OrdinalIgnoreCase));
        }
    }
}
