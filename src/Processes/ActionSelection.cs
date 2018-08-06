using System;
using System.Collections.Generic;
using System.Linq;

using Landis.SpatialModeling;

namespace Landis.Extension.SOSIELHuman.Processes
{
    using Entities;
    using Helpers;
    using Enums;
    using Exceptions;


    /// <summary>
    /// Action selection process implementation.
    /// </summary>
    public class ActionSelection : VolatileProcess
    {
        Goal processedGoal;
        GoalState goalState;


        Dictionary<Rule, Dictionary<Goal, double>> anticipatedInfluence;

        Rule[] matchedRules;


        Rule priorPeriodActivatedRule;
        Rule ruleForActivating;

        #region Specific logic for tendencies
        protected override void EqualToOrAboveFocalValue()
        {
            //Rule[] selected = new Rule[] { };

            //if (goalState.DiffCurrentAndFocal > 0)
            //{
            //    if (matchedRules.Any(r => r == priorPeriodActivatedRule))
            //    {
            //        ruleForActivating = priorPeriodActivatedRule;
            //        return;
            //    }
            //    else
            //    {
            //        Rule[] temp = matchedRules.Where(r => anticipatedInfluence[r][processedGoal] >= 0).ToArray();

            //        selected = temp.Where(r=> anticipatedInfluence[r][processedGoal] < goalState.DiffCurrentAndFocal).ToArray();

            //        if(selected.Length == 0)
            //        {
            //            selected = temp.Where(r => anticipatedInfluence[r][processedGoal] <= goalState.DiffCurrentAndFocal).ToArray();
            //        }
            //    }
            //}
            //else
            //{
            //    Rule[] temp = matchedRules.Where(r => anticipatedInfluence[r][processedGoal] >= 0).ToArray();

            //    selected = temp.Where(r=> anticipatedInfluence[r][processedGoal] > goalState.DiffCurrentAndFocal).ToArray();

            //    if (selected.Length == 0)
            //    {
            //        selected = temp.Where(r => anticipatedInfluence[r][processedGoal] >= goalState.DiffCurrentAndFocal).ToArray();
            //    }
            //}

            //if (selected.Length > 0)
            //{
            //    selected = selected.GroupBy(r => anticipatedInfluence[r][processedGoal]).OrderBy(hg => hg.Key).First().ToArray();

            //    ruleForActivating = selected.RandomizeOne();
            //}





            //We don't do anything. Do nothing rule will be selected later.
        }

        protected override void EqualToOrBelowFocalValue()
        {
            //Rule[] selected = new Rule[] { };

            //if (goalState.DiffCurrentAndFocal < 0)
            //{
            //    if (matchedRules.Any(r => r == priorPeriodActivatedRule))
            //    {
            //        ruleForActivating = priorPeriodActivatedRule;
            //        return;
            //    }
            //    else
            //    {
            //        Rule[] temp = matchedRules.Where(r => anticipatedInfluence[r][processedGoal] <= 0).ToArray();

            //        selected = temp.Where(r => anticipatedInfluence[r][processedGoal] < Math.Abs(goalState.DiffCurrentAndFocal)).ToArray();

            //        if (selected.Length == 0)
            //        {
            //            selected = temp.Where(r => anticipatedInfluence[r][processedGoal] <= Math.Abs(goalState.DiffCurrentAndFocal)).ToArray();
            //        }

            //    }
            //}
            //else
            //{
            //    Rule[] temp = matchedRules.Where(r => anticipatedInfluence[r][processedGoal] <= 0).ToArray();

            //    selected = temp.Where(r => anticipatedInfluence[r][processedGoal] > Math.Abs(goalState.DiffCurrentAndFocal)).ToArray();

            //    if (selected.Length == 0)
            //    {
            //        selected = temp.Where(r => anticipatedInfluence[r][processedGoal] >= Math.Abs(goalState.DiffCurrentAndFocal)).ToArray();
            //    }
            //}

            //if (selected.Length > 0)
            //{
            //    selected = selected.GroupBy(r => anticipatedInfluence[r][processedGoal]).OrderBy(hg => hg.Key).First().ToArray();

            //    ruleForActivating = selected.RandomizeOne();
            //}

            throw new NotImplementedException("EqualToOrBelowFocalValue is not implemented in ActionSelection");
        }

        protected override void Maximize()
        {
            if (matchedRules.Length > 0)
            {
                Rule[] selected = matchedRules.GroupBy(r => anticipatedInfluence[r][processedGoal]).OrderByDescending(hg => hg.Key).First().ToArray();

                ruleForActivating = selected.RandomizeOne();
            }
        }

        protected override void Minimize()
        {
            if (matchedRules.Length > 0)
            {
                Rule[] selected = matchedRules.GroupBy(r => anticipatedInfluence[r][processedGoal]).OrderBy(hg => hg.Key).First().ToArray();

                ruleForActivating = selected.RandomizeOne();
            }
        }
        #endregion

        /// <summary>
        /// Shares collective action among same household agents
        /// </summary>
        /// <param name="currentAgent"></param>
        /// <param name="rule"></param>
        /// <param name="agentStates"></param>
        void ShareCollectiveAction(IAgent currentAgent, Rule rule, Dictionary<IAgent, AgentState> agentStates)
        {
            foreach (IAgent neighbour in currentAgent.ConnectedAgents
                .Where(connected => connected[VariablesUsedInCode.Household] == currentAgent[VariablesUsedInCode.Household]))
            {
                if (neighbour.AssignedRules.Contains(rule) == false)
                {
                    neighbour.AssignNewRule(rule, currentAgent.AnticipationInfluence[rule]);
                }
            }
        }

        /// <summary>
        /// Executes first part of action selection for specific agent and site
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="lastIteration"></param>
        /// <param name="rankedGoals"></param>
        /// <param name="processedRules"></param>
        /// <param name="site"></param>
        public void ExecutePartI(IAgent agent, LinkedListNode<Dictionary<IAgent, AgentState>> lastIteration,
            Goal[] rankedGoals, Rule[] processedRules, ActiveSite site)
        {
            ruleForActivating = null;

            AgentState agentState = lastIteration.Value[agent];
            AgentState priorPeriod = lastIteration.Previous?.Value[agent];

            //adds new rule history for specific site if it doesn't exist
            if (agentState.RuleHistories.ContainsKey(site) == false)
                agentState.RuleHistories.Add(site, new RuleHistory());

            RuleHistory history = agentState.RuleHistories[site];

            processedGoal = rankedGoals.First(g => processedRules.First().Layer.Set.AssociatedWith.Contains(g));
            goalState = agentState.GoalsState[processedGoal];

            matchedRules = processedRules.Except(history.BlockedRules).Where(h => h.IsMatch(agent)).ToArray();

            if (matchedRules.Length > 1)
            {
                priorPeriodActivatedRule = priorPeriod.RuleHistories[site].Activated.FirstOrDefault(r => r.Layer == processedRules.First().Layer);
                
                //set anticipated influence before execute specific logic
                anticipatedInfluence = agent.AnticipationInfluence;

                SpecificLogic(processedGoal.Tendency);

                //if none are identified, then choose the do-nothing heuristic.
                if (ruleForActivating == null)
                {
                    try
                    {
                        ruleForActivating = processedRules.Single(h => h.IsAction == false);
                    }
                    catch (InvalidOperationException)
                    {
                        throw new SosielAlgorithmException(string.Format("Rule for activating hasn't been found by {0}", agent.Id));
                    }
                }
            }
            else
                ruleForActivating = matchedRules[0];

            if (processedRules.First().Layer.Set.Layers.Count > 1)
                ruleForActivating.Apply(agent);


            if (ruleForActivating.IsCollectiveAction)
            {
                ShareCollectiveAction(agent, ruleForActivating, lastIteration.Value);
            }


            history.Activated.Add(ruleForActivating);
            history.Matched.AddRange(matchedRules);
        }

        /// <summary>
        /// Executes second part of action selection for specific site
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="lastIteration"></param>
        /// <param name="rankedGoals"></param>
        /// <param name="processedRules"></param>
        /// <param name="site"></param>
        public void ExecutePartII(IAgent agent, LinkedListNode<Dictionary<IAgent, AgentState>> lastIteration,
            Goal[] rankedGoals, Rule[] processedRules, ActiveSite site)
        {
            AgentState agentState = lastIteration.Value[agent];

            RuleHistory history = agentState.RuleHistories[site];

            RuleLayer layer = processedRules.First().Layer;


            Rule selectedRule = history.Activated.Single(r => r.Layer == layer);

            if (selectedRule.IsCollectiveAction)
            {
                //counting agents which selected this rule
                int numberOfInvolvedAgents = agent.ConnectedAgents.Where(connected=> agent[VariablesUsedInCode.Household] == connected[VariablesUsedInCode.Household])
                    .Count(a=> lastIteration.Value[a].RuleHistories[site].Activated.Any(rule=> rule == selectedRule));

                int requiredParticipants = selectedRule.RequiredParticipants - 1;

                //add rule to blocked rules
                if (numberOfInvolvedAgents < requiredParticipants)
                {
                    history.BlockedRules.Add(selectedRule);

                    history.Activated.Remove(selectedRule);

                    ExecutePartI(agent, lastIteration, rankedGoals, processedRules, site);

                    ExecutePartII(agent, lastIteration, rankedGoals, processedRules, site);
                }
            }
        }
    }
}

