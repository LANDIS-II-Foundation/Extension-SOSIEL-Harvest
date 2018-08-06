using System;
using System.Collections.Generic;
using System.Linq;
using Landis.SpatialModeling;

namespace Landis.Extension.SOSIELHuman.Entities
{
    using Exceptions;
    using Helpers;
    

    public sealed class AgentState
    {
        public Dictionary<Goal, GoalState> GoalsState { get; private set; }

        public Dictionary<ActiveSite, RuleHistory> RuleHistories { get; private set; }

        public Dictionary<ActiveSite, List<TakenAction>> TakenActions { get; private set; }


        public bool IsSiteOriented { get; private set; }


        private AgentState()
        {
            GoalsState = new Dictionary<Goal, GoalState>();

            RuleHistories = new Dictionary<ActiveSite, RuleHistory>();

            TakenActions = new Dictionary<ActiveSite, List<TakenAction>>();
        }


        /// <summary>
        /// Creates empty agent state
        /// </summary>
        /// <param name="isSiteOriented"></param>
        /// <returns></returns>
        public static AgentState Create(bool isSiteOriented)
        {
            return new AgentState { IsSiteOriented = isSiteOriented };
        }



        /// <summary>
        /// Creates agent state with one rule history. For not site oriented agents only.
        /// </summary>
        /// <param name="isSiteOriented"></param>
        /// <param name="history"></param>
        /// <returns></returns>
        public static AgentState Create(bool isSiteOriented, RuleHistory history)
        {
            if (isSiteOriented)
                throw new SosielAlgorithmException("Wrong AgentState.Create method usage");

            AgentState state = Create(isSiteOriented);

            state.RuleHistories.Add(default(ActiveSite), history); 

            return state;
        }

        /// <summary>
        /// Creates agent state with rule histories related to sites.
        /// </summary>
        /// <param name="isSiteOriented"></param>
        /// <param name="history"></param>
        /// <returns></returns>
        public static AgentState Create(bool isSiteOriented, Dictionary<ActiveSite, RuleHistory> history)
        {
            AgentState state = Create(isSiteOriented);

            state.RuleHistories = new Dictionary<ActiveSite, RuleHistory>(history);

            return state;
        }


        /// <summary>
        /// Adds rule history to list. Can be used for not site oriented agents.
        /// </summary>
        /// <param name="history"></param>
        public void AddRuleHistory(RuleHistory history)
        {
            if (IsSiteOriented)
                throw new SosielAlgorithmException("Couldn't add rule history without site. It isn't possible for site oriented agents.");

            RuleHistories.Add(default(ActiveSite), history);
        }


        /// <summary>
        /// Adds rule history to list. Can be used for site oriented agents.
        /// </summary>
        /// <param name="history"></param>
        /// <param name="site"></param>
        public void AddRuleHistory(RuleHistory history, ActiveSite site)
        {
            RuleHistories.Add(site, history);
        }

        /// <summary>
        /// Creates new instance of agent site with copied anticipation influence and goals state from current state
        /// </summary>
        /// <returns></returns>
        public AgentState CreateForNextIteration()
        {
            AgentState agentState = Create(IsSiteOriented);

            GoalsState.ForEach(kvp =>
            {
                agentState.GoalsState.Add(kvp.Key, kvp.Value.CreateForNextIteration());
            });

            RuleHistories.Keys.ForEach(site =>
            {
                agentState.RuleHistories.Add(site, new RuleHistory());
            });

            return agentState;
        }
    }
}
