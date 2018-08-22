using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPDManager
{
    public class Rules
    {
        public string Rule { get; private set; }
        public string SqlForDel { get; private set; }
        public override string ToString()
        {
            return Rule;
        }

    }
}
