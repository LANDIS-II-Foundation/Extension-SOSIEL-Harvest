// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

using System.Collections.Generic;
using System.Linq;

using Landis.Extension.SOSIELHarvest.Configuration;
using Landis.Extension.SOSIELHarvest.Helpers;
using Landis.Extension.SOSIELHarvest.Models;
using Landis.Extension.SOSIELHarvest.Services;

//using Newtonsoft.Json;

using SOSIEL.Algorithm;
using SOSIEL.Configuration;
using SOSIEL.Entities;
using SOSIEL.Exceptions;
using SOSIEL.Helpers;
using SOSIEL.Processes;
using SOSIEL.VBGP.Input;
using SOSIEL.VBGP.Processes;

namespace Landis.Extension.SOSIELHarvest.Algorithm
{
    public class SosielHarvestAlgorithm : SosielAlgorithm, IAlgorithm<SosielData>
    {
        public string Name { get { return "SosielHarvestImplementation"; } }

        public List<IAgent> ActiveAgents => agentList.ActiveAgents.ToList();

        public Probabilities Probabilities => probabilities;

        private readonly LogService _log;
        private readonly int _sheMode;
        private readonly ConfigurationModel _configuration;
        private SosielData _algorithmModel;
        private readonly Area[] _activeAreas;
        private IReadOnlyDictionary<uint, SpeciesBiomassRecord> _initialSpeciesBiomass;
        private IReadOnlyDictionary<uint, SpeciesBiomassRecord> _currentSpeciesBiomass;

        /// <summary>
        /// Initializes Luhy lite implementation
        /// </summary>
        /// <param name="numberOfIterations">Number of internal iterations</param>
        /// <param name="configuration">Parsed agent configuration</param>
        /// <param name="areas">Enumerable of active areas from Landis</param>
        public SosielHarvestAlgorithm(
            LogService logService, int sheMode, int numberOfIterations, ConfigurationModel configuration,
            IEnumerable<Area> areas, GoalPrioritizingConfiguration goalPrioritizingConfiguration)
            : base(numberOfIterations,
                  ProcessesConfiguration.GetProcessesConfiguration(configuration.AlgorithmConfiguration.CognitiveLevel),
                  new Area(),
                  CreateGoalPrioritizingProcess(goalPrioritizingConfiguration))
        {
            _log = logService;
            _sheMode = sheMode;
            _configuration = configuration;
            _activeAreas = areas.ToArray();
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
        public void Run(SosielData data)
        {
            RunSosiel(_activeAreas);
        }

        public void SetSpeciesBiomass(IReadOnlyDictionary<uint, SpeciesBiomassRecord> speciesBiomass)
        {
            _currentSpeciesBiomass = speciesBiomass;
            if (PlugIn.ModelCore.CurrentTime == 0)
                _initialSpeciesBiomass = speciesBiomass;
        }

        /// <summary>
        /// Executes agent initializing. It's the first initializing step.
        /// </summary>
        protected override void InitializeAgents()
        {
            // Debugger.Launch();
            _log.WriteLine("  SosielHarvestAlgorithm: Initializing agents...");
            var agents = new List<IAgent>();
            var agentArchetypes = _configuration.AgentArchetypeConfiguration;
            if (agentArchetypes.Count == 0)
                throw new SosielAlgorithmException("Agent archetypes are not defined. Please check configuration files");

            // Create agents, groupby is used for saving agents numeration, e.g. FE1, HM1, HM2, etc.
            _configuration.InitialState.AgentStates.Where(s => s.Mode == _sheMode)
                .GroupBy(state => state.Archetype)
                .ForEach((agentStateGroup) =>
            {
                int index = 1;
                var archetype = agentArchetypes[agentStateGroup.Key];
                var mentalProto = archetype.MentalProto; //do not remove
                agentStateGroup.ForEach((agentState) =>
                {
                    for (var i = 0; i < agentState.NumberOfAgents; i++)
                    {
                        var name = agentState.Name;
                        if (string.IsNullOrEmpty(name) || agentState.NumberOfAgents > 1)
                            name = $"{agentState.Archetype}{index}";
                        var agent = SosielHarvestAgent.Create(agentState, archetype, name);
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

            agentList = new AgentList(agents, agentArchetypes.Select(kvp => kvp.Value).ToList());
            numberOfAgentsAfterInitialize = agentList.Agents.Count;
        }

        private static IGoalPrioritizing CreateGoalPrioritizingProcess(GoalPrioritizingConfiguration goalPrioritizingConfiguration)
        {
            switch (goalPrioritizingConfiguration.GoalPrioritizingType)
            {
                case GoalPrioritizingType.VBGP:
                {
                    var config = VBGPConfigurationReader.ReadConfiguration(goalPrioritizingConfiguration.ConfigFile);
                    return new ValueBasedGoalPrioritizing(config);
                }
                default: return null;
            }
        }

        private void InitializeProbabilities()
        {
            _log.WriteLine("  SosielHarvestAlgorithm: Initializing probabilities...");
            foreach (var probabilityElementConfiguration in 
                _configuration.AlgorithmConfiguration.ProbabilitiesConfiguration)
            {
                var variableType = VariableTypeHelper.ConvertStringToType(
                    probabilityElementConfiguration.VariableType);
                var parseTableMethod = ReflectionHelper.GetGenericMethod(
                    variableType, typeof(ProbabilityTableParser), "Parse");
                // Debugger.Launch();
                dynamic table = parseTableMethod.Invoke(null, 
                    new object[]
                    {
                        probabilityElementConfiguration.FilePath,
                        probabilityElementConfiguration.WithHeader
                    });
                var addToListMethod =
                    ReflectionHelper.GetGenericMethod(variableType, typeof(Probabilities), "AddProbabilityTable");
                addToListMethod.Invoke(
                    probabilities, new object[] { probabilityElementConfiguration.Variable, table });
            }
        }

        protected override void UseDemographic()
        {
            _log.WriteLine("  SosielHarvestAlgorithm: Enabling demographics...");
            base.UseDemographic();
            demographic = new SOSIEL.Processes.Demographic(_configuration.AlgorithmConfiguration.DemographicConfiguration,
                probabilities.GetProbabilityTable<int>(SosielProbabilityTables.BirthProbabilityTable),
                probabilities.GetProbabilityTable<int>(SosielProbabilityTables.DeathProbabilityTable));
        }

        /// <summary>
        /// Executes iteration state initializing. Executed after InitializeAgents.
        /// </summary>
        /// <returns></returns>
        ///
        protected override Dictionary<IAgent, AgentState> InitializeFirstIterationState()
        {
            _log.WriteLine("  SosielHarvestAlgorithm: Initializing first iteration...");
            var states = new Dictionary<IAgent, AgentState>();
            agentList.Agents.ForEach(agent =>
            {
                // Creates empty agent state
                var agentState = AgentState.Create(agent.Archetype.IsDataSetOriented);
                if (agent.Archetype.NamePrefix == "FM")
                {
                    UpdateSpeciesBiomassAgentVariables(agent, "Initial", _initialSpeciesBiomass);
                    UpdateSpeciesBiomassAgentVariables(agent, "Current", _currentSpeciesBiomass);
                }

                // Copy generated goal importance
                agent.InitialGoalStates.ForEach(kvp =>
                {
                    var goalState = kvp.Value;
                    goalState.Value = agent[kvp.Key.ReferenceVariable];
                    agentState.GoalStates[kvp.Key] = goalState;
                });

                states.Add(agent, agentState);
            });

            return states;
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
                        .ManagementAreaHarvested[HarvestResults.GetKey(_sheMode, fm, area)]).Average();
                }

                fm[AlgorithmVariables.ManageAreaMaturityPercent] = manageAreas.Select(
                    area => _algorithmModel.HarvestResults
                    .ManagementAreaMaturityPercent[HarvestResults.GetKey(_sheMode, fm, area)]).Average();

                fm[AlgorithmVariables.ManageAreaBiomass]  = manageAreas.Select(
                    area => _algorithmModel.HarvestResults
                    .ManagementAreaBiomass[HarvestResults.GetKey(_sheMode, fm, area)]).Sum();

                UpdateSpeciesBiomassAgentVariables(fm, "Current", _currentSpeciesBiomass);
            });
        }

        private void UpdateSpeciesBiomassAgentVariables(
            IAgent agent,
            string varPrefix,
            IReadOnlyDictionary<uint, SpeciesBiomassRecord> speciesBiomass
        )
        {
            if (speciesBiomass == null) return;
            foreach (var r in speciesBiomass)
            {
                foreach (var species in PlugIn.ModelCore.Species)
                {
                    var varName = $"{varPrefix}TotalAboveGroundSpeciesBiomass_{species.Name}_{r.Key}";
                    agent[varName] = r.Value.TotalAboveGroundBiomass[species.Index];
                    // _log.WriteLine($"SOSIELHarvestAlgorithm: Set '{agent.Id}'.'{varName}'={agent[varName]}");

                    varName = $"{varPrefix}AverageAboveGroundSpeciesBiomass_{species.Name}_{r.Key}";
                    agent[varName] = r.Value.AverageAboveGroundBiomass[species.Index];
                    // _log.WriteLine($"SOSIELHarvestAlgorithm: Set '{agent.Id}'.'{varName}'={agent[varName]}");
                }
            }
        }

        protected override void BeforeCounterfactualThinking(IAgent agent, IDataSet dataSet)
        {
            base.BeforeCounterfactualThinking(agent, dataSet);
            if (agent.Archetype.NamePrefix == "FM")
            {
                agent[AlgorithmVariables.ManageAreaBiomass] = _algorithmModel.HarvestResults
                    .ManagementAreaBiomass[HarvestResults.GetKey(_sheMode, agent, dataSet)];
            };
        }

        /// <summary>
        /// Executes before action selection process.
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="dataSet"></param>
        protected override void BeforeActionSelection(IAgent agent, IDataSet dataSet)
        {
            // Call default implementation.
            base.BeforeActionSelection(agent, dataSet);

            // If agent is FE, set to local variables current site biomass.
            if (agent.Archetype.NamePrefix == "FM")
            {
                // Set value of current area manage biomass to agent variable.
                agent[AlgorithmVariables.ManageAreaBiomass] = _algorithmModel.HarvestResults
                        .ManagementAreaBiomass[HarvestResults.GetKey(_sheMode, agent, dataSet)];
            }
        }

        protected override void PostIterationCalculations(int iteration)
        {
            var iterationState = iterations.Last.Value;
            var fmAgents = agentList.GetAgentsWithPrefix("FM");
            var iterationSelection = new Dictionary<string, List<string>>();

            foreach (var fmAgent in fmAgents)
            {
                var decisionOptionHistories = iterationState[fmAgent].DecisionOptionHistories;
                foreach (var area in decisionOptionHistories.Keys)
                {
                    if (!iterationSelection.TryGetValue(
                        HarvestResults.GetKey(_sheMode, fmAgent, area), out List<string> areaList))
                    {
                        areaList = new List<string>();
                        iterationSelection.Add(HarvestResults.GetKey(_sheMode, fmAgent, area), areaList);
                    }

                    // Not sure what to do with 2 or more similar DO from different agents
                    areaList.AddRange(decisionOptionHistories[area].Activated.Select(d => d.Name));
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

        protected override void AfterInnovation(IAgent agent, IDataSet dataSet, DecisionOption newDecisionOption)
        {
            base.AfterInnovation(agent, dataSet, newDecisionOption);
            if (newDecisionOption == null) return;
            var newDecisionOptionModel = new NewDecisionOptionModel()
            {
                ManagementArea = dataSet.Name,
                Name = newDecisionOption.Name,
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
        //protected override void PostIterationStatistic(int iteration)
        //{
        //    base.PostIterationStatistic(iteration);

        //    try
        //    {
        //        var settings = new JsonSerializerSettings()
        //        {
        //            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        //        };
        //        var data = JsonConvert.SerializeObject(iterations.Last.Value, settings);
        //        File.WriteAllText($"output_SOSIEL_Harvest_DUMP_{iteration}.json", data);
        //    }
        //    catch(Exception)
        //    {
        //    }

        //    // Save statistics for each agent
        //    agentList.ActiveAgents.ForEach(agent =>
        //    {
        //        var agentState = iterations.Last.Value[agent];
        //        if (agent.Archetype.NamePrefix == "FM")
        //        {
        //            foreach (var area in agentState.DecisionOptionHistories.Keys)
        //            {
        //                // Save activation rule stat
        //                var key = HarvestResults.GetKey(_sheMode, agent, area);
        //                var activatedDOs = agentState.DecisionOptionHistories[area]
        //                    .Activated.Distinct().OrderBy(r => r.Name).ToArray();
        //                var matchedDOs = agentState.DecisionOptionHistories[area]
        //                    .Matched.Distinct().OrderBy(r => r.Name).ToArray();
        //                var activatedDOIds = activatedDOs.Select(r => r.Name).ToArray();
        //                var matchedDOIds = matchedDOs.Select(r => r.Name).ToArray();
        //                var usage = new FMDOUsageOutput()
        //                {
        //                    Iteration = iteration,
        //                    ManagementArea = area.Name,
        //                    ActivatedDOValues = activatedDOs.Select(
        //                        r => string.IsNullOrEmpty(r.Consequent.VariableValue)
        //                        ? (string)r.Consequent.Value.ToString()
        //                        : (string)agent[r.Consequent.VariableValue].ToString()).ToArray(),
        //                    ActivatedDO = activatedDOIds,
        //                    MatchedDO = matchedDOIds,
        //                    MostImportantGoal = agentState.RankedGoals.First().Name,
        //                    TotalNumberOfDO = agent.AssignedDecisionOptions.Count,
        //                    BiomassHarvested = _algorithmModel.HarvestResults.ManagementAreaHarvested[key],
        //                    ManageAreaMaturityPercent = _algorithmModel.HarvestResults.ManagementAreaMaturityPercent[key],
        //                    Biomass = _algorithmModel.HarvestResults.ManagementAreaBiomass[key]
        //                };
        //                CSVHelper.AppendTo(string.Format("output_SOSIEL_Harvest_{0}.csv", agent.Id), usage);
        //            }
        //        }
        //    });
        //}

        //protected override IDataSet[] FilterManagementDataSets(IAgent agent, IDataSet[] orderedDataSets)
        //{
        //    var agentName = agent.Id;
        //    return orderedDataSets.Where(s => (s as Area).AssignedAgents.Contains(agentName)).ToArray();
        //}
    }
}
