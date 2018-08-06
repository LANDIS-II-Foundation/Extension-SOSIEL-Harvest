using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

namespace Landis.Extension.SOSIELHuman.Entities
{
    public class RuleSetConfiguration
    {
        public string[] AssociatedWith { get; private set; }

        public bool IsSequential { get; private set; }

        public Dictionary<string, RuleLayerConfiguration> Layer { get; private set; }
    }
}
