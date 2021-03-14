using System.Collections.Generic;
using System.Linq;
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
    }
}
