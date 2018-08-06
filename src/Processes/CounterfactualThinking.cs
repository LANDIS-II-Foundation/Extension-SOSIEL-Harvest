using System;
using System.Collections.Generic;
using System.Linq;

using Landis.SpatialModeling;

namespace Landis.Extension.SOSIELHuman.Processes
{
    using Entities;

    /// <summary>
    /// Counterfactual thinking process implementation.
    /// </summary>
    public class CounterfactualThinking : VolatileProcess
    {
        bool confidence;

        Goal selectedGoal;
        GoalState selectedGoalState;
        //HeuristicLayer layer;
        Dictionary<Rule, Dictionary<Goal, double>> anticipatedInfluences;

        Rule[] matchedRules;
        Rule activatedRule;

        #region Specific logic for tendencies
        protected override void EqualToOrAboveFocalValue()
        {
            Rule[] rules = anticipatedInfluences.Where(kvp=> matchedRules.Contains(kvp.Key))
                .Where(kvp => kvp.Value[selectedGoal] >= 0 && kvp.Value[selectedGoal] > selectedGoalState.DiffCurrentAndFocal).Select(kvp => kvp.Key).ToArray();

            //If 0 heuristics are identified, then heuristic-set-layer specific counterfactual thinking(t) = unsuccessful.
            if (rules.Length == 0)
            {
                confidence = false;
            }
            else
            {
                rules = rules.GroupBy(r => anticipatedInfluences[r][selectedGoal]).OrderBy(h => h.Key).First().ToArray();

                confidence = rules.Any(r => !(r == activatedRule || r.IsAction == false));
            }
        }

        protected override void EqualToOrBelowFocalValue()
        {
            Rule[] rules = anticipatedInfluences.Where(kvp => matchedRules.Contains(kvp.Key))
                .Where(kvp => kvp.Value[selectedGoal] < 0 && Math.Abs(kvp.Value[selectedGoal]) > Math.Abs(selectedGoalState.DiffCurrentAndFocal)).Select(kvp => kvp.Key).ToArray();

            
            //If 0 heuristics are identified, then heuristic-set-layer specific counterfactual thinking(t) = unsuccessful.
            if (rules.Length == 0)
            {
                confidence = false;
            }
            else
            {
                rules = rules.GroupBy(r => anticipatedInfluences[r][selectedGoal]).OrderBy(h => h.Key).First().ToArray();

                confidence = rules.Any(r => !(r == activatedRule || r.IsAction == false));
            }
        }

        protected override void Maximize()
        {
            Rule[] rules = anticipatedInfluences.Where(kvp => matchedRules.Contains(kvp.Key))
                .Where(kvp => kvp.Value[selectedGoal] >= 0).Select(kvp => kvp.Key).ToArray();

            //If 0 heuristics are identified, then heuristic-set-layer specific counterfactual thinking(t) = unsuccessful.
            if (rules.Length == 0)
            {
                confidence = false;
            }
            else
            {
                rules = rules.GroupBy(r => anticipatedInfluences[r][selectedGoal]).OrderByDescending(h => h.Key).First().ToArray();

                confidence = rules.Any(r => !(r == activatedRule || r.IsAction == false));
            }
        }

        protected override void Minimize()
        {
            throw new NotImplementedException("Minimize is not implemented in CounterfactualThinking");
        }
        #endregion


        /// <summary>
        /// Executes counterfactual thinking about most important agent goal for specific site
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="lastIteration"></param>
        /// <param name="goal"></param>
        /// <param name="matched"></param>
        /// <param name="layer"></param>
        /// <param name="site"></param>
        /// <returns></returns>
        public bool Execute(IAgent agent, LinkedListNode<Dictionary<IAgent, AgentState>> lastIteration, Goal goal,
            Rule[] matched, RuleLayer layer, ActiveSite site)
        {
            confidence = false;

            //Period currentPeriod = periodModel.Value;
            AgentState priorIterationAgentState = lastIteration.Previous.Value[agent];

            selectedGoal = goal;

            selectedGoalState = lastIteration.Value[agent].GoalsState[selectedGoal];

            RuleHistory history = priorIterationAgentState.RuleHistories[site];


            activatedRule = history.Activated.First(r => r.Layer == layer);

            anticipatedInfluences = agent.AnticipationInfluence;

            matchedRules = matched;

            SpecificLogic(selectedGoal.Tendency);


            return confidence;
        }
    }
}
