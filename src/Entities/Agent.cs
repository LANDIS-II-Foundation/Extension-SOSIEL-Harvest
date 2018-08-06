using System;
using System.Collections.Generic;
using System.Linq;


namespace Landis.Extension.SOSIELHuman.Entities
{
    using Configuration;
    using Environments;
    using Exceptions;
    using Helpers;

    public class Agent : IAgent, ICloneable<Agent>, IEquatable<Agent>
    {
        private int id;

        private Dictionary<string, dynamic> privateVariables { get; set; }

        public string Id { get { return Prototype.NamePrefix + id; } }

        public AgentPrototype Prototype { get; private set; }

        public List<IAgent> ConnectedAgents { get; set; }

        public Dictionary<Rule, Dictionary<Goal, double>> AnticipationInfluence { get; private set; }

        public List<Rule> AssignedRules { get; private set; }

        public List<Goal> AssignedGoals { get; private set; }

        public Dictionary<Rule, int> RuleActivationFreshness { get; private set; }

        public AgentStateConfiguration InitialStateConfiguration { get; private set; }

        public override string ToString()
        {
            return Id;
        }



        /// <summary>
        /// Closed agent constructor
        /// </summary>
        private Agent()
        {
            privateVariables = new Dictionary<string, dynamic>();
            ConnectedAgents = new List<IAgent>();
            AnticipationInfluence = new Dictionary<Rule, Dictionary<Goal, double>>();
            AssignedRules = new List<Rule>();
            AssignedGoals = new List<Goal>();
            RuleActivationFreshness = new Dictionary<Rule, int>();
        }


        public virtual dynamic this[string key]
        {
            get
            {
                if (privateVariables.ContainsKey(key))
                {
                    return privateVariables[key];
                }
                else
                {
                    if (Prototype.CommonVariables.ContainsKey(key))
                        return Prototype[key];
                }


                throw new UnknownVariableException(key);
            }
            set
            {
                if (privateVariables.ContainsKey(key) || Prototype.CommonVariables.ContainsKey(key))
                    PreSetValue(key, privateVariables[key]);

                if (Prototype.CommonVariables.ContainsKey(key))
                    Prototype[key] = value;
                else
                    privateVariables[key] = value;

                PostSetValue(key, value);
            }

        }


        /// <summary>
        /// Creates copy of current agent, after cloning need to set Id, connected agents don't copied
        /// </summary>
        /// <returns></returns>
        public Agent Clone()
        {
            Agent agent = new Agent();

            agent.Prototype = Prototype;
            agent.privateVariables = new Dictionary<string, dynamic>(privateVariables);

            agent.AssignedGoals = new List<Goal>(AssignedGoals);
            agent.AssignedRules = new List<Rule>(AssignedRules);

            //copy ai
            AnticipationInfluence.ForEach(kvp =>
            {
                agent.AnticipationInfluence.Add(kvp.Key, new Dictionary<Goal, double>(kvp.Value));
            });

            agent.RuleActivationFreshness = new Dictionary<Rule, int>(RuleActivationFreshness);

            return agent;
        }


        /// <summary>
        /// Checks on parameter existence 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsVariable(string key)
        {
            return privateVariables.ContainsKey(key) || Prototype.CommonVariables.ContainsKey(key);
        }


        /// <summary>
        /// Set variable value to prototype variables
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetToCommon(string key, dynamic value)
        {
            Prototype.CommonVariables[key] = value;
        }


        /// <summary>
        /// Handling variable after set to variables
        /// </summary>
        /// <param name="variable"></param>
        /// <param name="newValue"></param>
        protected virtual void PostSetValue(string variable, dynamic newValue)
        {

        }


        /// <summary>
        /// Handling variable before set to variables
        /// </summary>
        /// <param name="variable"></param>
        /// <param name="oldValue"></param>
        protected virtual void PreSetValue(string variable, dynamic oldValue)
        {

        }






        /// <summary>
        /// Assigns new rule to mental model (rule list) of current agent. If empty rooms ended, old rules will be removed.
        /// </summary>
        /// <param name="newRule"></param>
        public void AssignNewRule(Rule newRule)
        {
            RuleLayer layer = newRule.Layer;

            Rule[] layerRules = AssignedRules.GroupBy(r => r.Layer).Where(g => g.Key == layer).SelectMany(g => g).ToArray();

            if (layerRules.Length < layer.LayerConfiguration.MaxNumberOfRules)
            {
                AssignedRules.Add(newRule);
                AnticipationInfluence.Add(newRule, new Dictionary<Goal, double>());

                RuleActivationFreshness[newRule] = 0;
            }
            else
            {
                Rule ruleForRemoving = RuleActivationFreshness.Where(kvp => kvp.Key.Layer == layer && kvp.Key.IsAction).GroupBy(kvp => kvp.Value).OrderByDescending(g => g.Key)
                    .Take(1).SelectMany(g => g.Select(kvp => kvp.Key)).RandomizeOne();

                AssignedRules.Remove(ruleForRemoving);
                AnticipationInfluence.Remove(ruleForRemoving);

                RuleActivationFreshness.Remove(ruleForRemoving);

                AssignNewRule(newRule);
            }
        }

        /// <summary>
        /// Assigns new rule with defined anticipated influence to mental model (rule list) of current agent. If empty rooms ended, old rules will be removed. 
        /// Anticipated influence is copied to the agent.
        /// </summary>
        /// <param name="newRule"></param>
        /// <param name="anticipatedInfluence"></param>
        public void AssignNewRule(Rule newRule, Dictionary<Goal, double> anticipatedInfluence)
        {
            AssignNewRule(newRule);

            //copy ai to personal ai for assigned goals only

            Dictionary<Goal, double> ai = anticipatedInfluence.Where(kvp => AssignedGoals.Contains(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            AnticipationInfluence[newRule] = new Dictionary<Goal, double>(ai);
        }

        /// <summary>
        /// Adds rule to prototype rules and then assign one to the rule list of current agent.
        /// </summary>
        /// <param name="newRule"></param>
        /// <param name="layer"></param>
        public void AddRule(Rule newRule, RuleLayer layer)
        {
            Prototype.AddNewRule(newRule, layer);

            AssignNewRule(newRule);
        }


        /// <summary>
        /// Adds rule to prototype rules and then assign one to the rule list of current agent. 
        /// Also copies anticipated influence to the agent.
        /// </summary>
        /// <param name="newRule"></param>
        /// <param name="layer"></param>
        /// <param name="anticipatedInfluence"></param>
        public void AddRule(Rule newRule, RuleLayer layer, Dictionary<Goal, double> anticipatedInfluence)
        {
            Prototype.AddNewRule(newRule, layer);

            AssignNewRule(newRule, anticipatedInfluence);
        }


        /// <summary>
        /// Creates agent instance based on agent prototype and agent configuration 
        /// </summary>
        /// <param name="agentConfiguration"></param>
        /// <param name="prototype"></param>
        /// <returns></returns>
        public static Agent CreateAgent(AgentStateConfiguration agentConfiguration, AgentPrototype prototype)
        {
            Agent agent = new Agent();

            agent.Prototype = prototype;
            agent.privateVariables = new Dictionary<string, dynamic>(agentConfiguration.PrivateVariables);

            agent.AssignedRules = prototype.Rules.Where(r => agentConfiguration.AssignedRules.Contains(r.Id)).ToList();
            agent.AssignedGoals = prototype.Goals.Where(g => agentConfiguration.AssignedGoals.Contains(g.Name)).ToList();

            agent.AssignedRules.ForEach(rule => agent.RuleActivationFreshness.Add(rule, 1));


            //initializes initial anticipated influence for each rule and goal assigned to the agent
            agent.AssignedRules.ForEach(r =>
            {
                Dictionary<string, double> source;

                if (r.AutoGenerated && agent.Prototype.DoNothingAnticipatedInfluence != null)
                {
                    source = agent.Prototype.DoNothingAnticipatedInfluence;
                }
                else
                {
                    agentConfiguration.AnticipatedInfluenceState.TryGetValue(r.Id, out source);
                }


                Dictionary<Goal, double> inner = new Dictionary<Goal, double>();

                agent.AssignedGoals.ForEach(g =>
                {
                    inner.Add(g, source != null && source.ContainsKey(g.Name) ? source[g.Name] : 0);
                });

                agent.AnticipationInfluence.Add(r, inner);
            });


            agent.InitialStateConfiguration = agentConfiguration;

            return agent;
        }


        /// <summary>
        /// Sets id to current agent instance.
        /// </summary>
        /// <param name="id"></param>
        public void SetId(int id)
        {
            this.id = id;
        }



        /// <summary>
        /// Equality checking.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(Agent other)
        {
            return ReferenceEquals(this, other)
                || (other != null && Id == other.Id);
        }


        public override bool Equals(object obj)
        {
            return base.Equals(obj) || Equals(obj as Agent);
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public static bool operator ==(Agent a, Agent b)
        {
            if (Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            return a.Equals(b);
        }

        public static bool operator !=(Agent a, Agent b)
        {
            return !(a == b);
        }
    }
}
