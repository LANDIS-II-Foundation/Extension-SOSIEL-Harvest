using System;
using System.Collections.Generic;
using System.Linq;

namespace Landis.Extension.SOSIELHuman.Entities
{
    using Helpers;   

    public class RuleLayer: IComparable<RuleLayer>
    {
        int RuleIndexer = 0;
        public int PositionNumber { get; set; }

        public RuleSet Set { get; set; }

        public RuleLayerConfiguration LayerConfiguration { get; private set; }

        public List<Rule> Rules { get; private set; }

        public RuleLayer(RuleLayerConfiguration configuration)
        {
            Rules = new List<Rule>(configuration.MaxNumberOfRules);
            LayerConfiguration = configuration;
        }

        public RuleLayer(RuleLayerConfiguration parameters, IEnumerable<Rule> rules) : this(parameters)
        {
            rules.ForEach(r => Add(r));
        }

        /// <summary>
        /// Adds rule to the rule set layer.
        /// </summary>
        /// <param name="rule"></param>
        public void Add(Rule rule)
        {
            RuleIndexer++;
            rule.RulePositionNumber = RuleIndexer;
            rule.Layer = this;
           
            Rules.Add(rule);
        }

        /// <summary>
        /// Removes rule from rule set layer.
        /// </summary>
        /// <param name="rule"></param>
        public void Remove(Rule rule)
        {
            rule.Layer = null;

            Rules.Remove(rule);
        }

        public int CompareTo(RuleLayer other)
        {
            return this == other ? 0 : other.PositionNumber > PositionNumber ? -1 : 1;
        }
    }
}
