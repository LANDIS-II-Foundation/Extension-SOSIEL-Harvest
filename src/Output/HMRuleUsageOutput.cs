using System;
using System.Collections.Generic;
using System.Linq;


namespace Landis.Extension.SOSIELHuman.Output
{
    public class HMRuleUsageOutput
    {
        public int Iteration { get; set; }

		public string[] ActivatedRuleValues { get; set; }
		
        public string[] ActivatedRules { get; set; }

		public int TotalNumberOfRules { get; set; }
		
        public string[] NotActivatedRules { get; set; }
    }
}
