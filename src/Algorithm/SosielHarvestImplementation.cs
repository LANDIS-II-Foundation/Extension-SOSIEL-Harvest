using System;
using System.Collections.Generic;
using System.Linq;
using Landis.Extension.SOSIELHarvest.Configuration;
using Landis.Extension.SOSIELHarvest.Helpers;
using Landis.Extension.SOSIELHarvest.Output;
using SOSIEL.Algorithm;
using SOSIEL.Configuration;
using SOSIEL.Entities;
using SOSIEL.Exceptions;
using SOSIEL.Helpers;
using SOSIEL.Processes;

namespace Landis.Extension.SOSIELHarvest.Algorithm
{
    public class SosielHarvestImplementation : SosielAlgorithm<Area>, IAlgorithm<AlgorithmModel>
    {
        public string Name { get { return "SosielHarvestImplementation"; } }

        private ConfigurationModel configuration;
        private AlgorithmModel _algorithmModel;

        private Area[] activeAreas;

        private string _outputFolder;

        /// <summary>
        /// Initializes Luhy lite implementation
        /// </summary> 
        /// <param name="numberOfIterations">Number of internal iterations</param>
        /// <param name="configuration">Parsed agent configuration</param>
        /// <param name="areas">Enumerable of active areas from Landis</param>
        public SosielHarvestImplementation(int numberOfIterations,
            ConfigurationModel configuration,
            IEnumerable<Area> areas)
            : base(numberOfIterations,
                  ProcessesConfiguration.GetProcessesConfiguration(configuration.AlgorithmConfiguration.CognitiveLevel))
        {
            this.configuration = configuration;

            this.activeAreas = areas.ToArray();
        }

        /// <summary>
        /// Executes agent initializing. It's the first initializing step. 
        /// </summary>
        protected override void InitializeAgents()
        {
            var agents = new List<IAgent>();

            Dictionary<string, AgentArchetype> agentPrototypes = configuration.AgentConfiguration;

            if (agentPrototypes.Count == 0)
            {
                throw new SosielAlgorithmException("Agent prototypes were not defined. See configuration file");
            }

            InitialStateConfiguration initialState = configuration.InitialState;

            //create agents, groupby is used for saving agents numeration, e.g. FE1, HM1. HM2 etc
            initialState.AgentsState.GroupBy(state => state.PrototypeOfAgent).ForEach((agentStateGroup) =>
            {
                AgentArchetype archetype = agentPrototypes[agentStateGroup.Key];
                var mentalProto = archetype.MentalProto; //do not remove
                int index = 1;

                agentStateGroup.ForEach((agentState) =>
                {
                    for (int i = 0; i < agentState.NumberOfAgents; i++)
                    {
                        Agent agent = SosielHarvestAgent.CreateAgent(agentState, archetype);
                        agent.SetId(index);

                        agents.Add(agent);

                        index++;
                    }
                });
            });

            agentList = new AgentList(agents, agentPrototypes.Select(kvp => kvp.Value).ToList());

            numberOfAgentsAfterInitialize = agentList.Agents.Count;
        }

        private void InitializeProbabilities()
        {
            var probabilitiesList = new Probabilities();

            foreach (var probabilityElementConfiguration in configuration.AlgorithmConfiguration.ProbabilitiesConfiguration)
            {
                var variableType = VariableTypeHelper.ConvertStringToType(probabilityElementConfiguration.VariableType);
                var parseTableMethod = ReflectionHelper.GetGenerecMethod(variableType, typeof(ProbabilityTableParser), "Parse");

                dynamic table = parseTableMethod.Invoke(null, new object[] { probabilityElementConfiguration.FilePath, probabilityElementConfiguration.WithHeader });

                var addToListMethod =
                    ReflectionHelper.GetGenerecMethod(variableType, typeof(Probabilities), "AddProbabilityTable");

                addToListMethod.Invoke(probabilitiesList, new object[] { probabilityElementConfiguration.Variable, table });
            }

            probabilities = probabilitiesList;
        }

        protected override void UseDemographic()
        {
            base.UseDemographic();

            demographic = new Demographic<Area>(configuration.AlgorithmConfiguration.DemographicConfiguration,
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
            var states = new Dictionary<IAgent, AgentState<Area>>();

            agentList.Agents.ForEach(agent =>
            {
                //creates empty agent state
                AgentState<Area> agentState = AgentState<Area>.Create(agent.Archetype.IsSiteOriented);

                //copy generated goal importance
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
        /// Executes algorithm initialization
        /// </summary>
        public void Initialize(AlgorithmModel data)
        {
            _algorithmModel = data;

            InitializeAgents();

            InitializeProbabilities();

            if (configuration.AlgorithmConfiguration.UseDimographicProcesses)
            {
                UseDemographic();
            }

            AfterInitialization();
        }



        /// <summary>
        /// Runs as many internal iterations as passed to the constructor
        /// </summary>
        public AlgorithmModel Run(AlgorithmModel data)
        {
            RunSosiel(activeAreas);

            return data;
        }

        /// <summary>
        /// Executes last preparations before runs the algorithm. Executes after InitializeAgents and InitializeFirstIterationState.
        /// </summary>
        protected override void AfterInitialization()
        {
            //call default implementation
            base.AfterInitialization();

            //----
            //set default values which were not defined in configuration file

            var fmAgents = agentList.GetAgentsWithPrefix("FM");

            fmAgents.ForEach(fm =>
            {
                fm[AlgorithmVariables.MaxManageAreaHarvested] = 0d;
                fm[AlgorithmVariables.MinManageAreaHarvested] = 0d;
                fm[AlgorithmVariables.MaxManageAreaMaturityProportion] = 0d;
                fm[AlgorithmVariables.MinManageAreaMaturityProportion] = 0d;
            });
        }


        /// <summary>
        /// Executes at iteration start before any cognitive process is started.
        /// </summary>
        /// <param name="iteration"></param>
        protected override void PreIterationCalculations(int iteration)
        {
            //call default implementation
            base.PreIterationCalculations(iteration);

            _algorithmModel.NewDecisionOptions = new List<NewDecisionOptionModel>();

            if (iteration > 1)
            {
                var fmAgents = agentList.GetAgentsWithPrefix("FM");

                fmAgents.ForEach(fm =>
                {
                    var manageAreas = activeAreas.Where(a => a.AssignedAgents.Contains(fm.Id)).Select(a => a.Name).ToArray();

                    var maxHarvested = manageAreas.Select(a => _algorithmModel.HarvestResults.ManageAreaHarvested[a]).Max();
                    var maxMaturity = manageAreas.Select(a => _algorithmModel.HarvestResults.ManageAreaMaturityProportion[a]).Max();
                    
                    fm[AlgorithmVariables.ManageAreaHarvested] = manageAreas.Select(a => _algorithmModel.HarvestResults.ManageAreaHarvested[a]).Average();
                    fm[AlgorithmVariables.ManageAreaMaturityProportion] = manageAreas.Select(a => _algorithmModel.HarvestResults.ManageAreaMaturityProportion[a]).Average();

                    fm[AlgorithmVariables.MaxManageAreaHarvested] = Math.Max(maxHarvested, fm[AlgorithmVariables.MaxManageAreaHarvested]);
                    fm[AlgorithmVariables.MaxManageAreaMaturityProportion] = Math.Max(maxMaturity, fm[AlgorithmVariables.MaxManageAreaMaturityProportion]);
                });
            }
        }

        protected override void BeforeCounterfactualThinking(IAgent agent, Area site)
        {
            base.BeforeCounterfactualThinking(agent, site);

            if (agent.Archetype.NamePrefix == "FM")
            {
                agent[AlgorithmVariables.ManageAreaBiomass] = _algorithmModel.HarvestResults.ManageAreaBiomass[site.Name];
            };
        }


        /// <summary>
        /// Executes before action selection process
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="site"></param>
        protected override void BeforeActionSelection(IAgent agent, Area site)
        {
            //call default implementation
            base.BeforeActionSelection(agent, site);

            //if agent is FE, set to local variables current site biomass
            if (agent.Archetype.NamePrefix == "FM")
            {
                //set value of current area manage biomass to agent variable. 
                agent[AlgorithmVariables.ManageAreaBiomass] = _algorithmModel.HarvestResults.ManageAreaBiomass[site.Name];
            }
        }

        /// <summary>
        /// Executes after action taking process
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="site"></param>
        protected override void AfterActionTaking(IAgent agent, Area site)
        {
            //call default implementation
            base.AfterActionTaking(agent, site);


            if (agent.Archetype.NamePrefix == "FM")
            {
                //compute profit
                //add computed profit to total profit
                //agent[AlgorithmVariables.Profit] += profit;

                //reduce biomass
                //biomass[site] -= profit;
            }
        }

        protected override void PostIterationCalculations(int iteration)
        {
            var iterationState = iterations.Last.Value;

            var fmAgents = agentList.GetAgentsWithPrefix("FM");

            var iterationSelection = new Dictionary<string, List<string>>();

            foreach (var fmAgent in fmAgents)
            {
                var decisionOptionHistries = iterationState[fmAgent].DecisionOptionsHistories;

                foreach (var area in decisionOptionHistries.Keys)
                {
                    List<string> areaList;
                    if (!iterationSelection.TryGetValue(area.Name, out areaList))
                    {
                        areaList = new List<string>();
                        iterationSelection.Add(area.Name, areaList);
                    }

                    //not sure what to do with 2 or more similar DO from different agents
                    areaList.AddRange(decisionOptionHistries[area].Activated.Select(d => d.Id));
                }
            }

            _algorithmModel.SelectedDecisions = iterationSelection;

            base.PostIterationCalculations(iteration);
        }


        protected override void AfterInnovation(IAgent agent, Area site, DecisionOption newDecisionOption)
        {
            base.AfterInnovation(agent, site, newDecisionOption);

            if (newDecisionOption == null) return;

            var newDecisionOptionModel = new NewDecisionOptionModel()
            {
                ManagementArea = site.Name,
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
        /// Executes after PostIterationCalculations. Here we can collect all output data.
        /// </summary>
        /// <param name="iteration"></param>
        protected override void PostIterationStatistic(int iteration)
        {
            base.PostIterationStatistic(iteration);


            //save statistics for each agent
            agentList.ActiveAgents.ForEach(agent =>
            {
                AgentState<Area> agentState = iterations.Last.Value[agent];

                if (agent.Archetype.NamePrefix == "FM")
                {
                    foreach (var area in agentState.DecisionOptionsHistories.Keys)
                    {
                        //save activation rule stat
                        DecisionOption[] activatedRules = agentState.DecisionOptionsHistories[area].Activated.Distinct().OrderBy(r => r.Id).ToArray();

                        string[] activatedRuleIds = activatedRules.Select(r => r.Id).ToArray();

                        FMDOUsageOutput ruleUsage = new FMDOUsageOutput()
                        {
                            Iteration = iteration,
                            ManagementArea = area.Name,
                            ActivatedDOValues = activatedRules.Select(r => string.IsNullOrEmpty(r.Consequent.VariableValue) ? (string)r.Consequent.Value.ToString() : (string)agent[r.Consequent.VariableValue].ToString()).ToArray(),
                            ActivatedDO = activatedRuleIds,
                            TotalNumberOfDO = agent.AssignedDecisionOptions.Count
                        };

                        CSVHelper.AppendTo(string.Format("SOSIELHuman_{0}_rules.csv", agent.Id), ruleUsage);
                    }
                }
            });
        }

        protected override void Maintenance()
        {
            base.Maintenance();

            //clean up unassigned rules
            agentList.Archetypes.ForEach(prototype =>
            {
                IEnumerable<IAgent> agents = agentList.GetAgentsWithPrefix(prototype.NamePrefix);

                IEnumerable<DecisionOption> prototypeRules = prototype.MentalProto.SelectMany(mental => mental.AsDecisionOptionEnumerable()).ToArray();
                IEnumerable<DecisionOption> assignedRules = agents.SelectMany(agent => agent.AssignedDecisionOptions).Distinct();

                IEnumerable<DecisionOption> unassignedRules = prototypeRules.Except(assignedRules).ToArray();

                unassignedRules.ForEach(rule =>
                {
                    var layer = rule.Layer;

                    layer.Remove(rule);
                });
            });
        }

        protected override Area[] FilterManagementSites(IAgent agent, Area[] orderedSites)
        {
            var agentName = agent.Id;
            return orderedSites.Where(s => s.AssignedAgents.Contains(agentName)).ToArray();
        }
    }
}
