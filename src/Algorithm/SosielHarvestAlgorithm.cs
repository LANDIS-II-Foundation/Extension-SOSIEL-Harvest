﻿/// Name: SosielHarvestImplementation.cs
/// Description: 
/// Authors: Multiple.
/// Copyright: Garry Sotnik, Brooke A. Cassell, Robert M. Scheller.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Landis.Extension.SOSIELHarvest.Configuration;
using Landis.Extension.SOSIELHarvest.Helpers;
using Landis.Extension.SOSIELHarvest.Models;
using Landis.Extension.SOSIELHarvest.Output;
using Landis.Extension.SOSIELHarvest.Services;

using Newtonsoft.Json;
using SOSIEL.Algorithm;
using SOSIEL.Configuration;
using SOSIEL.Entities;
using SOSIEL.Exceptions;
using SOSIEL.Helpers;
using SOSIEL.Processes;
using AgentArchetype = SOSIEL.Entities.AgentArchetype;

namespace Landis.Extension.SOSIELHarvest.Algorithm
{
    public class SosielHarvestAlgorithm : SosielAlgorithm<Area>, IAlgorithm<SosielData>
    {
        public string Name { get { return "SosielHarvestImplementation"; } }

        public List<IAgent> ActiveAgents => agentList.ActiveAgents.ToList();

        public Probabilities Probabilities => probabilities;

        private LogService _logService;
        private ConfigurationModel _configuration;
        private SosielData _algorithmModel;
        private Area[] _activeAreas;

        /// <summary>
        /// Initializes Luhy lite implementation
        /// </summary>
        /// <param name="numberOfIterations">Number of internal iterations</param>
        /// <param name="configuration">Parsed agent configuration</param>
        /// <param name="areas">Enumerable of active areas from Landis</param>
        public SosielHarvestAlgorithm(
            LogService logService, int numberOfIterations, ConfigurationModel configuration, IEnumerable<Area> areas)
            : base(numberOfIterations,
                  ProcessesConfiguration.GetProcessesConfiguration(configuration.AlgorithmConfiguration.CognitiveLevel))
        {
            _logService = logService;
            _configuration = configuration;
            _activeAreas = areas.ToArray();
        }

        /// <summary>
        /// Executes agent initializing. It's the first initializing step.
        /// </summary>
        protected override void InitializeAgents()
        {
            _logService.WriteLine("  SosielHarvestAlgorithm: Initializing agents...");

            var agents = new List<IAgent>();

            Dictionary<string, AgentArchetype> agentPrototypes = _configuration.AgentConfiguration;

            if (agentPrototypes.Count == 0)
            {
                throw new SosielAlgorithmException("Agent prototypes were not defined. See configuration file");
            }

            InitialStateConfiguration initialState = _configuration.InitialState;

            // Create agents, groupby is used for saving agents numeration, e.g. FE1, HM1, HM2, etc.
            initialState.AgentsState.GroupBy(state => state.PrototypeOfAgent).ForEach((agentStateGroup) =>
            {
                AgentArchetype archetype = agentPrototypes[agentStateGroup.Key];
                var mentalProto = archetype.MentalProto; //do not remove
                int index = 1;

                agentStateGroup.ForEach((agentState) =>
                {
                    for (var i = 0; i < agentState.NumberOfAgents; i++)
                    {
                        var name = agentState.Name;

                        if (string.IsNullOrEmpty(name) || agentState.NumberOfAgents > 1)
                            name = $"{agentState.PrototypeOfAgent}{index}";

                        Agent agent = SosielHarvestAgent.CreateAgent(agentState, archetype, name);

                        agents.Add(agent);

                        index++;
                    }
                });

                agents.ForEach(agent =>
                {
                    if (!agent.ContainsVariable(AlgorithmVariables.Group))
                        return;

                    agent.ConnectedAgents.AddRange(agents.Where(a => a != agent && a.ContainsVariable(AlgorithmVariables.Group)
                                                                                && a[AlgorithmVariables.Group] == agent[AlgorithmVariables.Group]));
                });
            });

            agentList = new AgentList(agents, agentPrototypes.Select(kvp => kvp.Value).ToList());

            numberOfAgentsAfterInitialize = agentList.Agents.Count;
        }

        private void InitializeProbabilities()
        {
            _logService.WriteLine("  SosielHarvestAlgorithm: Initializing probabilities...");

            var probabilitiesList = new Probabilities();

            foreach (var probabilityElementConfiguration in _configuration.AlgorithmConfiguration.ProbabilitiesConfiguration)
            {
                var variableType = VariableTypeHelper.ConvertStringToType(probabilityElementConfiguration.VariableType);
                var parseTableMethod = ReflectionHelper.GetGenerecMethod(variableType, typeof(ProbabilityTableParser), "Parse");

                // Debugger.Launch();
                dynamic table = parseTableMethod.Invoke(null, new object[] { probabilityElementConfiguration.FilePath, probabilityElementConfiguration.WithHeader });

                var addToListMethod =
                    ReflectionHelper.GetGenerecMethod(variableType, typeof(Probabilities), "AddProbabilityTable");

                addToListMethod.Invoke(probabilitiesList, new object[] { probabilityElementConfiguration.Variable, table });
            }

            probabilities = probabilitiesList;
        }

        protected override void UseDemographic()
        {
            _logService.WriteLine("  SosielHarvestAlgorithm: Enabling demographics...");
            base.UseDemographic();
            demographic = new Demographic<Area>(_configuration.AlgorithmConfiguration.DemographicConfiguration,
                probabilities.GetProbabilityTable<int>(AlgorithmProbabilityTables.BirthProbabilityTable),
                probabilities.GetProbabilityTable<int>(AlgorithmProbabilityTables.DeathProbabilityTable));
        }

        /// <summary>
        /// Executes iteration state initializing. Executed after InitializeAgents.
        /// </summary>
        /// <returns></returns>
        ///
        protected override Dictionary<IAgent, AgentState<Area>> InitializeFirstIterationState()
        {
            _logService.WriteLine("  SosielHarvestAlgorithm: Initializing first iteration...");

            var states = new Dictionary<IAgent, AgentState<Area>>();

            agentList.Agents.ForEach(agent =>
            {
                // Creates empty agent state
                AgentState<Area> agentState = AgentState<Area>.Create(agent.Archetype.IsDataSetOriented);

                // Copy generated goal importance
                agent.InitialGoalStates.ForEach(kvp =>
                {
                    var goalState = kvp.Value;
                    goalState.Value = agent[kvp.Key.ReferenceVariable];

                    agentState.GoalsState[kvp.Key] = goalState;
                });

                states.Add(agent, agentState);
            });

            return states;
        }

        /// <summary>
        /// Executes algorithm initialization.
        /// </summary>
        public void Initialize(SosielData data)
        {
            _algorithmModel = data;
            InitializeAgents();
            InitializeProbabilities();
            if (_configuration.AlgorithmConfiguration.UseDimographicProcesses)
                UseDemographic();
            AfterInitialization();
        }

        /// <summary>
        /// Runs as many internal iterations as passed to the constructor.
        /// </summary>
        public SosielData Run(SosielData data)
        {
            RunSosiel(_activeAreas);
            return data;
        }

        /// <summary>
        /// Executes last preparations before running the algorithm. Executes after InitializeAgents and InitializeFirstIterationState.
        /// </summary>
        protected override void AfterInitialization()
        {
            // Call default implementation.
            base.AfterInitialization();

            //----
            // Set default values, which were not defined in configuration file.
            //var fmAgents = agentList.GetAgentsWithPrefix("FM");
        }


        /// <summary>
        /// Executes at iteration start before any cognitive process is started.
        /// </summary>
        /// <param name="iteration"></param>
        protected override void PreIterationCalculations(int iteration)
        {
            // Call default implementation.
            base.PreIterationCalculations(iteration);

            _algorithmModel.NewDecisionOptions = new List<NewDecisionOptionModel>();

            var fmAgents = agentList.GetAgentsWithPrefix("FM");
            fmAgents.ForEach(fm =>
            {
                var manageAreas = _activeAreas.Where(a => a.AssignedAgents.Contains(fm.Id)).ToArray();

                fm[AlgorithmVariables.ManageAreaHarvested] = manageAreas.Select(area => _algorithmModel.HarvestResults.ManageAreaHarvested[HarvestResults.GetKey(_algorithmModel.Mode, fm, area)]).Average();
                fm[AlgorithmVariables.ManageAreaMaturityPercent] = manageAreas.Select(area => _algorithmModel.HarvestResults.ManageAreaMaturityPercent[HarvestResults.GetKey(_algorithmModel.Mode, fm, area)]).Average();
                fm[AlgorithmVariables.ManageAreaBiomass] = manageAreas.Select(area => _algorithmModel.HarvestResults.ManageAreaBiomass[HarvestResults.GetKey(_algorithmModel.Mode, fm, area)]).Sum();

                if (iteration == 1)
                {
                    fm[AlgorithmVariables.ManageAreaHarvested] = 0d;    // ?????? should be commented
                }
            });
        }

        protected override void BeforeCounterfactualThinking(IAgent agent, Area dataSet)
        {
            base.BeforeCounterfactualThinking(agent, dataSet);

            if (agent.Archetype.NamePrefix == "FM")
            {
                agent[AlgorithmVariables.ManageAreaBiomass] = _algorithmModel.HarvestResults.ManageAreaBiomass[HarvestResults.GetKey(_algorithmModel.Mode,agent, dataSet)];
            };
        }


        /// <summary>
        /// Executes before action selection process.
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="dataSet"></param>
        protected override void BeforeActionSelection(IAgent agent, Area dataSet)
        {
            // Call default implementation.
            base.BeforeActionSelection(agent, dataSet);

            // If agent is FE, set to local variables current site biomass.
            if (agent.Archetype.NamePrefix == "FM")
            {
                // Set value of current area manage biomass to agent variable.
                agent[AlgorithmVariables.ManageAreaBiomass] = _algorithmModel.HarvestResults.ManageAreaBiomass[HarvestResults.GetKey(_algorithmModel.Mode,agent, dataSet)];
            }
        }

        /// <summary>
        /// Executes after action taking process.
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="dataSet"></param>
        protected override void AfterActionTaking(IAgent agent, Area dataSet)
        {
            // Call default implementation.
            base.AfterActionTaking(agent, dataSet);


            if (agent.Archetype.NamePrefix == "FM")
            {
                // Compute profit
                // Add computed profit to total profit
                // Agent[AlgorithmVariables.Profit] += profit;

                // Reduce biomass
                // Biomass[site] -= profit;
            }
        }

        protected override void PostIterationCalculations(int iteration)
        {
            var iterationState = iterations.Last.Value;
            var fmAgents = agentList.GetAgentsWithPrefix("FM");
            var iterationSelection = new Dictionary<string, List<string>>();

            foreach (var fmAgent in fmAgents)
            {
                var decisionOptionHistories = iterationState[fmAgent].DecisionOptionsHistories;

                foreach (var area in decisionOptionHistories.Keys)
                {
                    if (!iterationSelection.TryGetValue(HarvestResults.GetKey(_algorithmModel.Mode, fmAgent, area), out List<string> areaList))
                    {
                        areaList = new List<string>();
                        iterationSelection.Add(HarvestResults.GetKey(_algorithmModel.Mode,fmAgent, area), areaList);
                    }

                    // Not sure what to do with 2 or more similar DO from different agents
                    areaList.AddRange(decisionOptionHistories[area].Activated.Select(d => d.Id));
                }
            }

            _algorithmModel.SelectedDecisions = iterationSelection;

            base.PostIterationCalculations(iteration);
        }

        protected override void AfterInnovation(IAgent agent, Area dataSet, DecisionOption newDecisionOption)
        {
            base.AfterInnovation(agent, dataSet, newDecisionOption);

            if (newDecisionOption == null) return;

            var newDecisionOptionModel = new NewDecisionOptionModel()
            {
                ManagementArea = dataSet.Name,
                Name = newDecisionOption.Id,
                ConsequentVariable = newDecisionOption.Consequent.Param,
                ConsequentValue = string.IsNullOrEmpty(newDecisionOption.Consequent.VariableValue)
                    ? newDecisionOption.Consequent.Value
                    : agent[newDecisionOption.Consequent.VariableValue],
                BasedOn = newDecisionOption.Origin
            };

            _algorithmModel.NewDecisionOptions.Add(newDecisionOptionModel);
        }


        /// <summary>
        /// Executes after PostIterationCalculations. Collects output data.
        /// </summary>
        /// <param name="iteration"></param>
        protected override void PostIterationStatistic(int iteration)
        {
            base.PostIterationStatistic(iteration);

            try
            {
                var settings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };
                var data = JsonConvert.SerializeObject(iterations.Last.Value, settings);
                File.WriteAllText($"output_SOSIEL_Harvest_DUMP_{iteration}.json", data);
            }
            catch(Exception)
            {
            }

            // Save statistics for each agent
            agentList.ActiveAgents.ForEach(agent =>
            {
                AgentState<Area> agentState = iterations.Last.Value[agent];
                if (agent.Archetype.NamePrefix == "FM")
                {
                    foreach (var area in agentState.DecisionOptionsHistories.Keys)
                    {
                        // Save activation rule stat
                        DecisionOption[] activatedDOs = agentState.DecisionOptionsHistories[area].Activated.Distinct().OrderBy(r => r.Id).ToArray();
                        DecisionOption[] matchedDOs = agentState.DecisionOptionsHistories[area].Matched.Distinct().OrderBy(r => r.Id).ToArray();

                        string[] activatedDOIds = activatedDOs.Select(r => r.Id).ToArray();
                        string[] matchedDOIds = matchedDOs.Select(r => r.Id).ToArray();

                        FMDOUsageOutput ruleUsage = new FMDOUsageOutput()
                        {
                            Iteration = iteration,
                            ManagementArea = area.Name,
                            ActivatedDOValues = activatedDOs.Select(r => string.IsNullOrEmpty(r.Consequent.VariableValue) ? (string)r.Consequent.Value.ToString() : (string)agent[r.Consequent.VariableValue].ToString()).ToArray(),
                            ActivatedDO = activatedDOIds,
                            MatchedDO = matchedDOIds,
                            MostImportantGoal = agentState.RankedGoals.First().Name,
                            TotalNumberOfDO = agent.AssignedDecisionOptions.Count,
                            BiomassHarvested = _algorithmModel.HarvestResults.ManageAreaHarvested[HarvestResults.GetKey(_algorithmModel.Mode,agent, area)],
                            ManageAreaMaturityPercent = _algorithmModel.HarvestResults.ManageAreaMaturityPercent[HarvestResults.GetKey(_algorithmModel.Mode,agent, area)],
                            Biomass = _algorithmModel.HarvestResults.ManageAreaBiomass[HarvestResults.GetKey(_algorithmModel.Mode,agent, area)]
                        };

                        CSVHelper.AppendTo(string.Format("output_SOSIEL_Harvest_{0}.csv", agent.Id), ruleUsage);
                    }
                }
            });
        }

        protected override Area[] FilterManagementDataSets(IAgent agent, Area[] orderedDataSets)
        {
            var agentName = agent.Id;
            return orderedDataSets.Where(s => s.AssignedAgents.Contains(agentName)).ToArray();
        }
    }
}