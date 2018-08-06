using System;
using System.Collections.Generic;
using System.Linq;


namespace Landis.Extension.SOSIELHuman.Entities
{
    using Exceptions;
    using Helpers;


    public class AgentPrototype
    {
        public string NamePrefix { get; private set; }

        public Dictionary<string, dynamic> CommonVariables { get; private set; }

        public List<Goal> Goals { get; private set; }

        public Dictionary<string, RuleSetConfiguration> SetConfiguration { get; private set; }

        public List<Rule> Rules { get; private set; }


        public Dictionary<string, double> DoNothingAnticipatedInfluence { get; private set; }


        private List<RuleSet> mentalProto;
        /// <summary>
        /// Rule sets transformed from rule list 
        /// </summary>
        public List<RuleSet> MentalProto
        {
            get { return mentalProto == null ? TransformRulesToRuleSets() : mentalProto; }
        }

        public bool IsSiteOriented { get; set; }

        public AgentPrototype()
        {
            CommonVariables = new Dictionary<string, dynamic>();
            SetConfiguration = new Dictionary<string, RuleSetConfiguration>();
            Rules = new List<Rule>();
        }

        public dynamic this[string key]
        {
            get
            {
                if (CommonVariables.ContainsKey(key))
                    return CommonVariables[key];

                throw new UnknownVariableException(key);
            }
            set
            {
                CommonVariables[key] = value;
            }

        }




        /// <summary>
        /// Transforms from rule list to rule sets
        /// </summary>
        /// <returns></returns>
        private List<RuleSet> TransformRulesToRuleSets()
        {
            mentalProto = Rules.GroupBy(r => r.RuleSet).OrderBy(g => g.Key).Select(g =>
                   new RuleSet(g.Key, Goals.Where(goal => SetConfiguration[g.Key.ToString()].AssociatedWith.Contains(goal.Name)).ToArray(),
                       g.GroupBy(r => r.RuleLayer).OrderBy(g2 => g2.Key).
                       Select(g2 => new RuleLayer(SetConfiguration[g.Key.ToString()].Layer[g2.Key.ToString()], g2)))).ToList();

            return mentalProto;
        }



        /// <summary>
        /// Adds rule to rule scope of current prototype if it isn't exists in the scope.
        /// </summary>
        /// <param name="newRule"></param>
        /// <param name="layer"></param>
        public void AddNewRule(Rule newRule, RuleLayer layer)
        {
            if (mentalProto == null)
                TransformRulesToRuleSets();

            layer.Add(newRule);

            Rules.Add(newRule);
        }


        /// <summary>
        /// Checks for similar rules
        /// </summary>
        /// <param name="rule"></param>
        /// <returns></returns>
        public bool IsSimilarRuleExists(Rule rule)
        {
            return Rules.Any(r => r == rule);
        }


        /// <summary>
        /// Adds do nothing rule to each rule set and rule layer
        /// </summary>
        /// <returns>Returns created rule ids</returns>
        public IEnumerable<string> AddDoNothingRules()
        {
            List<string> temp = new List<string>();

            MentalProto.ForEach(set =>
            {
                //only for layers with UseDoNothing: true configuration
                set.Layers.Where(layer => layer.LayerConfiguration.UseDoNothing).ForEach(layer =>
                {
                    if (!layer.Rules.Any(r => r.IsAction == false))
                    {
                        Rule proto = layer.Rules.First();

                        Rule doNothing = Rule.Create(
                            new RuleAntecedentPart[] { new RuleAntecedentPart(VariablesUsedInCode.IsActive, "==", true) },
                            RuleConsequent.Renew(proto.Consequent, Activator.CreateInstance(proto.Consequent.Value.GetType())),
                            false, false, 1, true
                        );

                        AddNewRule(doNothing, layer);

                        temp.Add(doNothing.Id);
                    }
                });
            });

            return temp;
        }

    }
}
