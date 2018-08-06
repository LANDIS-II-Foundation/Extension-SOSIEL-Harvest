using System;
using System.Collections.Generic;
using System.Linq;


namespace Landis.Extension.SOSIELHuman.Entities
{
    public class RuleHistory
    {
        public List<Rule> Matched { get; private set; }
        public List<Rule> Activated { get; private set; }

        public List<Rule> BlockedRules { get; private set; }

        public RuleHistory()
        {
            Matched = new List<Rule>();
            Activated = new List<Rule>();

            BlockedRules = new List<Rule>();
        }

        public RuleHistory(IEnumerable<Rule> matched, IEnumerable<Rule> activated) : base()
        {
            Matched.AddRange(matched);
            Activated.AddRange(activated);
        }
    }
}
