using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPDManager
{
    public class ISItem
    {
        public Int32 IntValue { get; set; }
        public string StrValue { get; set; }
        public override string ToString()
        {
            return StrValue;
        }
    }
}
