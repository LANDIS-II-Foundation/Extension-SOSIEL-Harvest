// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

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

namespace Landis.Extension.SOSIELHarvest.Algorithm
{
    public class SosielHarvestAlgorithm : SosielAlgorithm<Area>, IAlgorithm<SosielData>
    {
        public string Name { get { return "SosielHarvestImplementation"; } }

        public List<IAgent> ActiveAgents => agentList.ActiveAgents.ToList();

        public Probabilities Probabilities => probabilities;

        private readonly LogService _log;
        private readonly int _sheMode;
        private readonly ConfigurationModel _configuration;
        private SosielData _algorithmModel;
        private readonly Area[] _activeAreas;
        private IReadOnlyList<SpeciesBiomassRecord> _speciesBiomassRecords;

        /// <summary>
        /// Initializes Luhy lite implementation
        /// </summary>
        /// <param name="numberOfIterations">Number of internal iterations</param>
        /// <param name="configuration">Parsed agent configuration</param>
        /// <param name="areas">Enumerable of active areas from Landis</param>
        public SosielHarvestAlgorithm(
            LogService logService, int sheMode, int numberOfIterations, ConfigurationModel configuration,
            IEnumerable<Area> areas)
            : base(numberOfIterations,
                  ProcessesConfiguration.GetProcessesConfiguration(configuration.AlgorithmConfiguration.CognitiveLevel))
        {
            _log = logService;
            _sheMode = sheMode;
            _configuration = configuration;
            _activeAreas = areas.ToArray();
        }

        public void SetSpeciesBiomass(IReadOnlyList<SpeciesBiomassRecord> speciesBiomassRecords)
        {
            _speciesBiomassRecords = speciesBiomassRecords;
        }

        /// <summary>
        /// Executes agent initializing. It's the first initializing step.
        /// </summary>
        protected override void InitializeAgents()
        {
            // Debugger.Launch();
            _log.WriteLine("  SosielHarvestAlgorithm: Initializing agents...");
            var agents = new List<IAgent>();
            var agentPrototypes = _configuration.AgentConfiguration;
            if (agentPrototypes.Count == 0)
                throw new SosielAlgorithmException("Agent prototypes were not defined. See configuration file");

            // Create agents, groupby is used for saving agents numeration, e.g. FE1, HM1, HM2, etc.
            _configuration.InitialState.AgentsState.Where(s => s.Mode == _sheMode)
                .GroupBy(state => state.PrototypeOfAgent)
                .ForEach((agentStateGroup) =>
            {
                int index = 1;
                var archetype = agentPrototypes[agentStateGroup.Key];
                var mentalProto = archetype.MentalProto; //do not remove
                agentStateGroup.ForEach((agentState) =>
                {
                    for (var i = 0; i < agentState.NumberOfAgents; i++)
                    {
                        var name = agentState.Name;
                        if (string.IsNullOrEmpty(name) || agentState.NumberOfAgents > 1)
                            name = $"{agentState.PrototypeOfAgent}{index}";
                        var agent = SosielHarvestAgent.CreateAgent(agentState, archetype, name);
                        agents.Add(agent);
                        index++;
                    }
                });

                agents.ForEach(agent =>         
                {
                    if (agent.ContainsVariable(AlgorithmVariables.Group))
                    {
                        agent.ConnectedAgents.AddRange(agents.Where(
                            a => a != agent && a.ContainsVariable(AlgorithmVariables.Group)
                                 && a[AlgorithmVariables.Group] == agent[AlgorithmVariables.Group]));
                    }
                });
            });

            agentList = new AgentList(agents, agentPrototypes.Select(kvp => kvp.Value).ToList());
            numberOfAgentsAfterInitialize = agentList.Agents.Count;
        }

        private void InitializeProbabilities()
        {
            _log.WriteLine("  SosielHarvestAlgorithm: Initializing probabilities...");
            var probabilitiesList = new Probabilities();
            foreach (var probabilityElementConfiguration in 
                _configuration.AlgorithmConfiguration.ProbabilitiesConfiguration)
            {
                var variableType = VariableTypeHelper.ConvertStringToType(
                    probabilityElementConfiguration.VariableType);
                var parseTableMethod = ReflectionHelper.GetGenerecMethod(
                    variableType, typeof(ProbabilityTableParser), "Parse");
                // Debugger.Launch();
                dynamic table = parseTableMethod.Invoke(null, 
                    new object[]
                    {
                        probabilityElementConfiguration.FilePath,
                        probabilityElementConfiguration.WithHeader
                    });
                var addToListMethod =
                    ReflectionHelper.GetGenerecMethod(variableType, typeof(Probabilities), "AddProbabilityTable");
                addToListMethod.Invoke(
                    probabilitiesList, new object[] { probabilityElementConfiguration.Variable, table });
            }
            probabilities = probabilitiesList;
        }

        protected override void UseDemographic()
        {
            _log.WriteLine("  SosielHarvestAlgorithm: Enabling demographics...");
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
            _log.WriteLine("  SosielHarvestAlgorithm: Initializing first iteration...");
            var states = new Dictionary<IAgent, AgentState<Area>>();
            agentList.Agents.ForEach(agent =>
            {
                // Creates empty agent state
                var agentState = AgentState<Area>.Create(agent.Archetype.IsDataSetOriented);
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
        /// Defines custom maintenance process.
        /// </summary>
        protected override void Maintenance()
        {
            base.Maintenance();

            // Reset personal monetary properties of the inactive agents
            foreach (var agent in agentList.Agents)
            {
                if (agent[SosielVariables.IsActive] != true)
                {
                    agent[AlgorithmVariables.AgentIncome] = 0.0;
                    agent[AlgorithmVariables.AgentExpenses] = 0.0;
                    agent[AlgorithmVariables.AgentSavings] = 0.0;
                }
            }
        }

        protected override void AfterInitialization()
        {
            var hmAgents = agentList.GetAgentsWithPrefix("HM");
            hmAgents.ForEach(agent =>
            {
                agent[AlgorithmVariables.AgentIncome] = 0.0;
                agent[AlgorithmVariables.AgentExpenses] = 0.0;
                agent[AlgorithmVariables.AgentSavings] = 0.0;
            });
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

                if (iteration == 1)
                    fm[AlgorithmVariables.ManageAreaHarvested] = 0.0;
                else
                {
                    fm[AlgorithmVariables.ManageAreaHarvested] = manageAreas.Select(
                        area => _algorithmModel.HarvestResults
                        .ManageAreaHarvested[HarvestResults.GetKey(_sheMode, fm, area)]).Average();
                }

                fm[AlgorithmVariables.ManageAreaMaturityPercent] = manageAreas.Select(
                    area => _algorithmModel.HarvestResults
                    .ManageAreaMaturityPercent[HarvestResults.GetKey(_sheMode, fm, area)]).Average();

                fm[AlgorithmVariables.ManageAreaBiomass]  = manageAreas.Select(
                    area => _algorithmModel.HarvestResults
                    .ManageAreaBiomass[HarvestResults.GetKey(_sheMode, fm, area)]).Sum();

                if (_speciesBiomassRecords != null)
                {
                    if (iteration == 1)
                    {
                        foreach (var r in _speciesBiomassRecords)
                        {
                            var varName = "InitialAverageAboveGroundSpeciesBiomass_" +
                                $"{r.Species.Name}_{r.EcoRegion.Name}";
                            fm[varName] = r.AverageAboveGroundBiomass;
                            // _log.WriteLine($"Set '{fm.Id}'.'{varName}'={r.AverageAboveGroundBiomass}");
                        }
                    }
                    foreach (var r in _speciesBiomassRecords)
                    {
                        var varName = $"CurrentAverageAboveGroundSpeciesBiomass_{r.Species.Name}_{r.EcoRegion.Name}";
                        fm[varName] = r.AverageAboveGroundBiomass;
                        // _log.WriteLine($"Set '{fm.Id}'.'{varName}'={r.AverageAboveGroundBiomass}");
                    }
                }
            });
        }

        protected override void BeforeCounterfactualThinking(IAgent agent, Area dataSet)
        {
            base.BeforeCounterfactualThinking(agent, dataSet);
            if (agent.Archetype.NamePrefix == "FM")
            {
                agent[AlgorithmVariables.ManageAreaBiomass] = _algorithmModel.HarvestResults
                    .ManageAreaBiomass[HarvestResults.GetKey(_sheMode, agent, dataSet)];
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
                agent[AlgorithmVariables.ManageAreaBiomass] = _algorithmModel.HarvestResults
                        .ManageAreaBiomass[HarvestResults.GetKey(_sheMode, agent, dataSet)];
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
                    if (!iterationSelection.TryGetValue(
                        HarvestResults.GetKey(_sheMode, fmAgent, area), out List<string> areaList))
                    {
                        areaList = new List<string>();
                        iterationSelection.Add(HarvestResults.GetKey(_sheMode, fmAgent, area), areaList);
                    }

                    // Not sure what to do with 2 or more similar DO from different agents
                    areaList.AddRange(decisionOptionHistories[area].Activated.Select(d => d.Id));
                }
            }

            _algorithmModel.SelectedDecisions = iterationSelection;

            base.PostIterationCalculations(iteration);

            // Update income and expense
            var hmAgents = agentList.GetAgentsWithPrefix("HM");
            hmAgents.GroupBy(agent => agent[SosielVariables.Household])
                .ForEach(householdAgents =>
                {
                    var householdIncome = householdAgents.Sum(agent => (double)agent[AlgorithmVariables.AgentIncome]);
                    var householdExpenses = householdAgents.Sum(
                        agent => (double)agent[AlgorithmVariables.AgentExpenses]);
                    var iterationHouseholdSavings = householdIncome - householdExpenses;
                    var householdSavings = householdAgents
                        .Where(agent => agent.ContainsVariable(AlgorithmVariables.HouseholdSavings))
                        .Select(agent => (double)agent[AlgorithmVariables.HouseholdSavings])
                        .FirstOrDefault() + iterationHouseholdSavings;

                    householdAgents.ForEach(agent =>
                    {
                        agent[AlgorithmVariables.HouseholdIncome] = householdIncome;
                        agent[AlgorithmVariables.HouseholdExpenses] = householdExpenses;
                        agent[AlgorithmVariables.HouseholdSavings] = householdSavings;
                    });
                });
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
                var agentState = iterations.Last.Value[agent];
                if (agent.Archetype.NamePrefix == "FM")
                {
                    foreach (var area in agentState.DecisionOptionsHistories.Keys)
                    {
                        // Save activation rule stat
                        var key = HarvestResults.GetKey(_sheMode, agent, area);
                        var activatedDOs = agentState.DecisionOptionsHistories[area]
                            .Activated.Distinct().OrderBy(r => r.Id).ToArray();
                        var matchedDOs = agentState.DecisionOptionsHistories[area]
                            .Matched.Distinct().OrderBy(r => r.Id).ToArray();
                        var activatedDOIds = activatedDOs.Select(r => r.Id).ToArray();
                        var matchedDOIds = matchedDOs.Select(r => r.Id).ToArray();
                        var usage = new FMDOUsageOutput()
                        {
                            Iteration = iteration,
                            ManagementArea = area.Name,
                            ActivatedDOValues = activatedDOs.Select(
                                r => string.IsNullOrEmpty(r.Consequent.VariableValue)
                                ? (string)r.Consequent.Value.ToString()
                                : (string)agent[r.Consequent.VariableValue].ToString()).ToArray(),
                            ActivatedDO = activatedDOIds,
                            MatchedDO = matchedDOIds,
                            MostImportantGoal = agentState.RankedGoals.First().Name,
                            TotalNumberOfDO = agent.AssignedDecisionOptions.Count,
                            BiomassHarvested = _algorithmModel.HarvestResults.ManageAreaHarvested[key],
                            ManageAreaMaturityPercent = _algorithmModel.HarvestResults.ManageAreaMaturityPercent[key],
                            Biomass = _algorithmModel.HarvestResults.ManageAreaBiomass[key]
                        };
                        CSVHelper.AppendTo(string.Format("output_SOSIEL_Harvest_{0}.csv", agent.Id), usage);
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
