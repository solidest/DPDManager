using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPDManager
{
    public class Rule
    {
        public string RuleText { get; set; }
        public List<Int32> RowList { get; set; }

        public string TableName { get; set; }

        public override string ToString()
        {
            return RuleText;
        }
    }
}
