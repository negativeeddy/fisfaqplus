using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Teams.Apps.FAQPlusPlus.ImportKb
{
    class ImportKbPoco
    {
        public string Question { get; set; }
        public string Answer { get; set; }
        [Optional]
        public string Source { get; set; }
        [Optional]
        public string Metadata { get; set; }
        [Optional]
        public string SuggestedQuestions { get; set; }
        [Optional]
        public bool IsContextOnly { get; set; }
        [Optional]
        public string Prompts { get; set; }
        [Optional]
        public int QnaId { get; set; }
    }
}
