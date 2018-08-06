using System;
using System.Collections.Generic;
using System.Linq;


namespace Landis.Extension.SOSIELHuman.Processes
{
    using Entities;
    using Helpers;


    /// <summary>
    /// Social learning process implementation.
    /// </summary>
    public class SocialLearning
    {
        /// <summary>
        /// Executes social learning process of current agent for specific rule set layer
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="lastIteration"></param>
        /// <param name="layer"></param>
        public void ExecuteLearning(IAgent agent, LinkedListNode<Dictionary<IAgent, AgentState>> lastIteration, RuleLayer layer)
        {
            Dictionary<IAgent, AgentState> priorIterationState = lastIteration.Previous.Value;

            agent.ConnectedAgents.Randomize().ForEach(neighbour =>
            {
                IEnumerable<Rule> activatedRules = priorIterationState[neighbour].RuleHistories
                    .SelectMany(rh => rh.Value.Activated).Where(r => r.Layer == layer);

                activatedRules.ForEach(rule =>
                {
                    if (agent.AssignedRules.Contains(rule) == false)
                    {
                        agent.AssignNewRule(rule, neighbour.AnticipationInfluence[rule]);
                    }
                });

            });
        }
    }
}
