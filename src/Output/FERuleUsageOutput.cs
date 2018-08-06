using System;
using System.Collections.Generic;
using System.Linq;


namespace Landis.Extension.SOSIELHuman.Output
{
    public class FERuleUsageOutput
    {
        public int Iteration { get; set; }

        public string Site { get; set; }

        public string[] ActivatedRules { get; set; }

        public string[] NotActivatedRules { get; set; }
    }
}
