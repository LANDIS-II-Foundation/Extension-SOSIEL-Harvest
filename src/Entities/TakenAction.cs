using System;
using System.Collections.Generic;
using System.Linq;


namespace Landis.Extension.SOSIELHuman.Entities
{
    public class TakenAction
    {
        public string RuleId { get; private set; }

        public string VariableName { get; private set; }

        public dynamic Value { get; private set; }


        public TakenAction(string ruleId, string variableName, dynamic value)
        {
            RuleId = ruleId;
            VariableName = variableName;
            Value = value;
        }

    }
}
