using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

using Landis.SpatialModeling;

namespace Landis.Extension.SOSIELHuman.Algorithm
{
	using Configuration;
	using Entities;
	using Helpers;
	using Processes;
	using Exceptions;

	public abstract class SosielAlgorithm
	{
		private int numberOfIterations;
        private int iterationCounter;
		private ProcessesConfiguration processConfiguration;

		protected int numberOfAgentsAfterInitialize; 
		protected bool algorithmStoppage = false;
		protected AgentList agentList;
		protected LinkedList<Dictionary<IAgent, AgentState>> iterations = new LinkedList<Dictionary<IAgent, AgentState>>();

		//processes
		AnticipatoryLearning al = new AnticipatoryLearning();
		CounterfactualThinking ct = new CounterfactualThinking();
		Innovation it = new Innovation();
		SocialLearning sl = new SocialLearning();
		ActionSelection acts = new ActionSelection();
		ActionTaking at = new ActionTaking();


		public SosielAlgorithm(int numberOfIterations, ProcessesConfiguration processConfiguration)
		{
			this.numberOfIterations = numberOfIterations;
			this.processConfiguration = processConfiguration;

            iterationCounter = 0;
		}

		/// <summary>
        /// Executes agent initializing. It's the first initializing step. 
        /// </summary>
        protected abstract void InitializeAgents();


        /// <summary>
        /// Executes iteration state initializing. Executed after InitializeAgents.
        /// </summary>
        /// <returns></returns>
        protected abstract Dictionary<IAgent, AgentState> InitializeFirstIterationState();


        /// <summary>
        /// Executes last preparations before runs the algorithm. Executes after InitializeAgents and InitializeFirstIterationState.
        /// </summary>
        protected virtual void AfterInitialization() { }

        /// <summary>
        /// Executes in the end of the algorithm.
        /// </summary>
        protected virtual void AfterAlgorithmExecuted() { }


        /// <summary>
        /// Executes before any cognitive process is started.
        /// </summary>
        /// <param name="iteration"></param>
        protected virtual void PreIterationCalculations(int iteration) { }


        /// <summary>
        /// Executes after PreIterationCalculations
        /// </summary>
        /// <param name="iteration"></param>
        protected virtual void PreIterationStatistic(int iteration) { }


        /// <summary>
        /// Executes before action selection process
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="site"></param>
        protected virtual void BeforeActionSelection(IAgent agent, ActiveSite site) { }


        /// <summary>
        /// Executes after action taking process
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="site"></param>
        protected virtual void AfterActionTaking(IAgent agent, ActiveSite site) { }


        /// <summary>
        /// Befores the counterfactual thinking.
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="site"></param>
        protected virtual void BeforeCounterfactualThinking(IAgent agent, ActiveSite site) { }


        /// <summary>
        /// Executes after last cognitive process is finished
        /// </summary>
        /// <param name="iteration"></param>
        protected virtual void PostIterationCalculations(int iteration) { }

        /// <summary>
        /// Executes after PostIterationCalculations
        /// </summary>
        /// <param name="iteration"></param>
        protected virtual void PostIterationStatistic(int iteration) { }

		/// <summary>
        /// Executes agent deactivation logic.
        /// </summary>
        protected virtual void AgentsDeactivation() { }

        /// <summary>
        /// Executed after AgentsDeactivation.
        /// </summary>
        /// <param name="iteration"></param>
        protected virtual void AfterDeactivation(int iteration) { }



		/// <summary>
        /// Executes reproduction logic.
        /// </summary>
        /// <param name="minAgentNumber"></param>
        protected virtual void Reproduction(int minAgentNumber)
		{
			
		}

        /// <summary>
        /// Executes maintenance logic.
        /// </summary>
        protected virtual void Maintenance()
		{
			agentList.ActiveAgents.ForEach(a =>
			{
                //increment rule activation freshness
				a.RuleActivationFreshness.Keys.ToArray().ForEach(k =>
				{
					a.RuleActivationFreshness[k] += 1;
				});
			});
		}


		/// <summary>
        /// Executes SOSIEL Algorithm
        /// </summary>
        /// <param name="activeSites"></param>
        protected void RunSosiel(IEnumerable<ActiveSite> activeSites)
		{
			for (int i = 1; i <= numberOfIterations; i++)
			{
                iterationCounter++;

				IAgent[] orderedAgents = agentList.ActiveAgents.Randomize(processConfiguration.AgentRandomizationEnabled).ToArray();

				var agentGroups = orderedAgents.GroupBy(a => a[VariablesUsedInCode.AgentType]).OrderBy(group => group.Key).ToArray();

				PreIterationCalculations(iterationCounter);
				PreIterationStatistic(iterationCounter);

				Dictionary<IAgent, AgentState> currentIteration;

				if (iterationCounter > 1)
					currentIteration = iterations.AddLast(new Dictionary<IAgent, AgentState>()).Value;
				else
					currentIteration = iterations.AddLast(InitializeFirstIterationState()).Value;

				Dictionary<IAgent, AgentState> priorIteration = iterations.Last.Previous?.Value;



				Dictionary<IAgent, Goal[]> rankedGoals = new Dictionary<IAgent, Goal[]>(agentList.Agents.Count);

				orderedAgents.ForEach(a =>
				{
					rankedGoals.Add(a, a.AssignedGoals.ToArray());

					if (iterationCounter > 1)
						currentIteration.Add(a, priorIteration[a].CreateForNextIteration());
				});


                ActiveSite[] orderedSites = activeSites.Randomize().ToArray();

                ActiveSite[] notSiteOriented = new ActiveSite[] { default(ActiveSite) };

				if (processConfiguration.AnticipatoryLearningEnabled && iterationCounter > 1)
				{
					//1st round: AL, CT, IR
					foreach (var agentGroup in agentGroups)
					{
						foreach (IAgent agent in agentGroup)
						{
							//anticipatory learning process
							rankedGoals[agent] = al.Execute(agent, iterations.Last);

							if (processConfiguration.CounterfactualThinkingEnabled == true)
							{
								if (rankedGoals[agent].Any(g => currentIteration[agent].GoalsState.Any(kvp => kvp.Value.Confidence == false)))
								{
                                    foreach (ActiveSite site in agent.Prototype.IsSiteOriented ? orderedSites : notSiteOriented)
                                    {
                                        BeforeCounterfactualThinking(agent, site);

                                        foreach (var set in agent.AssignedRules.GroupBy(h => h.Layer.Set).OrderBy((IGrouping<RuleSet, Rule> g) => g.Key.PositionNumber))
                                        {
                                            //optimization
                                            Goal selectedGoal = rankedGoals[agent].First(g => set.Key.AssociatedWith.Contains(g));

                                            GoalState selectedGoalState = currentIteration[agent].GoalsState[selectedGoal];

                                            if (selectedGoalState.Confidence == false)
                                            {
                                                foreach (var layer in set.GroupBy(h => h.Layer).OrderBy((IGrouping<RuleLayer, Rule> g) => g.Key.PositionNumber))
                                                {
                                                    if (layer.Key.LayerConfiguration.Modifiable || (!layer.Key.LayerConfiguration.Modifiable && layer.Any(r => r.IsModifiable)))
                                                    {
                                                        //looking for matched rules in prior period
                                                        Rule[] matchedPriorPeriodHeuristics = priorIteration[agent].RuleHistories[site]
                                                                .Matched.Where(h => h.Layer == layer.Key).ToArray();

                                                        bool? CTResult = null;

                                                        //counterfactual thinking process
                                                        if (matchedPriorPeriodHeuristics.Length >= 2)
                                                            CTResult = ct.Execute(agent, iterations.Last, selectedGoal, matchedPriorPeriodHeuristics, layer.Key, site);


                                                        if (processConfiguration.InnovationEnabled == true)
                                                        {
                                                            //innovation process
                                                            if (CTResult == false || matchedPriorPeriodHeuristics.Length < 2)

                                                                it.Execute(agent, iterations.Last, selectedGoal, layer.Key, site);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
								}
							}
						}
					}
				}

				if (processConfiguration.SocialLearningEnabled && iterationCounter > 1)
				{
					//2nd round: SL
					foreach (var agentGroup in agentGroups)
					{

						foreach (IAgent agent in agentGroup)
						{
							foreach (var set in agent.AssignedRules.GroupBy(h => h.Layer.Set).OrderBy(g => g.Key.PositionNumber))
							{
								foreach (var layer in set.GroupBy(h => h.Layer).OrderBy(g => g.Key.PositionNumber))
								{
                                    //social learning process
                                    sl.ExecuteLearning(agent, iterations.Last, layer.Key);
								}
							}
						}
					}

				}

				if (processConfiguration.RuleSelectionEnabled && iterationCounter > 1)
				{
					//AS part I
					foreach (var agentGroup in agentGroups)
					{
						foreach (IAgent agent in agentGroup)
						{
                            foreach (ActiveSite site in agent.Prototype.IsSiteOriented ? orderedSites : notSiteOriented)
                            {
                                foreach (var set in agent.AssignedRules.GroupBy(h => h.Layer.Set).OrderBy((IGrouping<RuleSet, Rule> g) => g.Key.PositionNumber))
                                {
                                    foreach (var layer in set.GroupBy(h => h.Layer).OrderBy((IGrouping<RuleLayer, Rule> g) => g.Key.PositionNumber))
                                    {
                                        
                                        BeforeActionSelection(agent, site);


                                        //action selection process part I
                                        acts.ExecutePartI(agent, iterations.Last, rankedGoals[agent], layer.ToArray(), site);
                                    }
                                }
                            }
						}
					}


					if (processConfiguration.RuleSelectionPart2Enabled && iterationCounter > 1)
					{
						//4th round: AS part II
						foreach (var agentGroup in agentGroups)
						{
							foreach (IAgent agent in agentGroup)
							{
                                foreach (ActiveSite site in agent.Prototype.IsSiteOriented ? orderedSites : notSiteOriented)
                                {
                                    foreach (var set in agent.AssignedRules.GroupBy(r => r.Layer.Set).OrderBy((IGrouping<RuleSet, Rule> g) => g.Key.PositionNumber))
                                    {
                                        foreach (var layer in set.GroupBy(h => h.Layer).OrderBy((IGrouping<RuleLayer, Rule> g) => g.Key.PositionNumber))
                                        {
                                            BeforeActionSelection(agent, site);

                                            //action selection process part II
                                            acts.ExecutePartII(agent, iterations.Last, rankedGoals[agent], layer.ToArray(), site);
                                        }
                                    }
                                }
							}
						}
					}
				}

				if (processConfiguration.ActionTakingEnabled)
				{
					//5th round: TA
					foreach (var agentGroup in agentGroups)
					{
						foreach (IAgent agent in agentGroup)
						{
                            foreach (ActiveSite site in agent.Prototype.IsSiteOriented ? orderedSites : notSiteOriented)
                            {
                                at.Execute(agent, currentIteration[agent], site);

                                AfterActionTaking(agent, site);
                            }
							//if (periods.Last.Value.IsOverconsumption)
							//    return periods;
						}
					}
				}

				if (processConfiguration.AlgorithmStopIfAllAgentsSelectDoNothing && iterationCounter > 1)
				{
					if (currentIteration.SelectMany(kvp => kvp.Value.RuleHistories.Values.SelectMany(rh=> rh.Activated)).All(r => r.IsAction == false))
					{
						algorithmStoppage = true;
					}
				}

				PostIterationCalculations(iterationCounter);

				PostIterationStatistic(iterationCounter);

				if (processConfiguration.AgentsDeactivationEnabled && iterationCounter > 1)
				{
					AgentsDeactivation();
				}

				AfterDeactivation(iterationCounter);

				if (processConfiguration.ReproductionEnabled && iterationCounter > 1)
				{
					Reproduction(0);
				}

				if (algorithmStoppage || agentList.ActiveAgents.Length == 0)
					break;

				Maintenance();
			}
		}
	}
}
