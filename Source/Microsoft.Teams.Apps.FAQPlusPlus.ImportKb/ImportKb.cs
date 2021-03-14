using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Azure.CognitiveServices.Knowledge.QnAMaker;
using Microsoft.Azure.CognitiveServices.Knowledge.QnAMaker.Models;
using Microsoft.Teams.Apps.FAQPlusPlus.Common;
using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Microsoft.Teams.Apps.FAQPlusPlus.ImportKb
{
    public partial class ImportKb : Form
    {
        public ImportKb()
        {
            InitializeComponent();
        }

        private void btnSelectKb_Click(object sender, EventArgs e)
        {
            var fileContent = string.Empty;
            var filePath = string.Empty;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "tsv files (*.tsv)|*.tsv|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    filePath = openFileDialog.FileName;
                    tbFileName.Text = filePath;
                }
            }
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            var ms = new MemoryStream();
            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = "\t",
                Encoding = UTF8Encoding.UTF8,
                HasHeaderRecord = true,
                MissingFieldFound = null,
                BadDataFound = null,
            };

            var qnaMakerClient = new QnAMakerClient(new ApiKeyServiceClientCredentials(ConfigurationManager.AppSettings["QnAMakerSubscriptionKey"]))
            { Endpoint = ConfigurationManager.AppSettings["QnAMakerApiEndpointUrl"] };
            var knowledgeBase = ConfigurationManager.AppSettings["QnAMakerKnowledgeBaseId"];
            var activityTableName = "ActivityEntity";

            using (var reader = new StreamReader(tbFileName.Text))
            using (var csv = new CsvReader(reader, csvConfig))
            {
                csv.Read();
                csv.ReadHeader();
                while (csv.Read())
                {
                    var record = csv.GetRecord<ImportKbPoco>();
                    var activityReferenceId = Guid.NewGuid().ToString();

                    var random = new Random();
                    var activityId = random.Next().ToString();

                    // Update knowledgebase.
                    qnaMakerClient.Knowledgebase.UpdateAsync(knowledgeBase, new UpdateKbOperationDTO
                    {
                        // Create JSON of changes.
                        Add = new UpdateKbOperationDTOAdd
                        {
                            QnaList = new List<QnADTO>
                            {
                                 new QnADTO
                                 {
                                    Questions = new List<string> { record.Question?.Trim() },
                                    Answer = record.Answer?.Trim(),
                                    Metadata = new List<MetadataDTO>()
                                    {
                                        new MetadataDTO() { Name = Constants.MetadataCreatedAt, Value = DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture) },
                                        new MetadataDTO() { Name = Constants.MetadataCreatedBy, Value = "21514982-6fdb-496e-81c0-5755209438bc" },
                                        new MetadataDTO() { Name = Constants.MetadataConversationId, Value = activityReferenceId },
                                        new MetadataDTO() { Name = Constants.MetadataActivityReferenceId, Value = activityReferenceId },
                                    },
                                 },
                            },
                        },
                        Update = null,
                        Delete = null,
                    }).ConfigureAwait(false);

                    tbQuestions.Text = tbQuestions.Text + $"Question added - : {record.Question}";

                    var storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);
                    var cloudTableClient = storageAccount.CreateCloudTableClient();
                    var cloudTable = cloudTableClient.GetTableReference(activityTableName);

                    var activityEntity = new ActivityEntity
                        { ActivityId = activityId, ActivityReferenceId = activityReferenceId };

                    var insertOrMergeOperation = TableOperation.InsertOrReplace(activityEntity);
                    var result =  cloudTable.ExecuteAsync(insertOrMergeOperation).ConfigureAwait(false);

                }


            }
        }
    }
}
