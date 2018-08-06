using System.Collections.Generic;

namespace Landis.Extension.SOSIELHuman.Entities
{
    using Configuration;
    using Environments;

    public interface IAgent
    {
        dynamic this[string key] { get; set; }

        string Id { get; }

        List<IAgent> ConnectedAgents { get; }

        Dictionary<Rule, Dictionary<Goal, double>> AnticipationInfluence { get; }

        List<Goal> AssignedGoals { get; }

        List<Rule> AssignedRules { get; }

        Dictionary<Rule, int> RuleActivationFreshness { get; }

        AgentPrototype Prototype { get; }



        AgentStateConfiguration InitialStateConfiguration { get; }

        /// <summary>
        /// Assigns new rule to mental model (rule list) of current agent. If empty rooms ended, old rules will be removed.
        /// </summary>
        /// <param name="newRule"></param>
        void AssignNewRule(Rule newRule);

        /// <summary>
        /// Assigns new rule with defined anticipated influence to mental model (rule list) of current agent. If empty rooms ended, old rules will be removed. 
        /// Anticipated influence is copied to the agent.
        /// </summary>
        /// <param name="newRule"></param>
        /// <param name="anticipatedInfluence"></param>
        void AssignNewRule(Rule newRule, Dictionary<Goal, double> anticipatedInfluence);

        /// <summary>
        /// Adds rule to prototype rules and then assign one to the rule list of current agent.
        /// </summary>
        /// <param name="newRule"></param>
        /// <param name="layer"></param>
        void AddRule(Rule newRule, RuleLayer layer);


        /// <summary>
        /// Adds rule to prototype rules and then assign one to the rule list of current agent. 
        /// Also copies anticipated influence to the agent.
        /// </summary>
        /// <param name="newRule"></param>
        /// <param name="layer"></param>
        /// <param name="anticipatedInfluence"></param>
        void AddRule(Rule newRule, RuleLayer layer, Dictionary<Goal, double> anticipatedInfluence);

        /// <summary>
        /// Set variable value to prototype variables
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void SetToCommon(string key, dynamic value);

        /// <summary>
        /// Check on parameter existence 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool ContainsVariable(string key);
    }
}