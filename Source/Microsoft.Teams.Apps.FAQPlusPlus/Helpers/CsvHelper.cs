using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.Teams.Apps.FAQPlusPlus.Helpers
{
    public class CSVHelper : List<string[]>
    {
        protected string csv = string.Empty;
        protected string separator = ",";
        protected bool header = true;

        public CSVHelper(string csv, string separator, bool header)
        {
            this.csv = csv;
            this.separator = separator;
            this.header = header;
            var firstLine = true;
            foreach (string line in Regex.Split(csv, System.Environment.NewLine).ToList().Where(s => !string.IsNullOrEmpty(s)))
            {
                string[] values = Regex.Split(line, separator);

                for (int i = 0; i < values.Length; i++)
                {
                    //Trim values
                    values[i] = values[i].Trim('\"');
                }
                if (header && firstLine)
                {
                    firstLine = false;
                    continue;
                }

                this.Add(values);
            }
        }

        public static IList<AnswerItem> AnswerListFromCsv(string fileContent)
        {
            CSVHelper csv = new CSVHelper(fileContent, ",", true);
            var answers = from string[] line in csv
                          select new AnswerItem
                          {
                              Question = line[0],
                              Answer = null,
                              Metadata = line[2],
                          };
            return answers.ToList();
        }

        public static byte[] CsvFromQuestions(IList<AnswerItem> questions)
        {
            var csvOut = new StringBuilder();
            var header = "Question,Answer,Metadata";
            csvOut.AppendLine(header);
            foreach (var answer in questions)
            {
                var q = answer.Question;
                var a = answer.Answer;
                var m = answer.Metadata;
                var qapair = string.Format("{0},{1},{2}", q, a, m);
                csvOut.AppendLine(qapair);
            }

            var csvString = csvOut.ToString();
            var bytes = UTF8Encoding.UTF8.GetBytes(csvString);
            return bytes;
        }
    }
}
