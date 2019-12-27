using System.Collections.Generic;
using System.Linq;
using Landis.Extension.SOSIELHarvest.Configuration;
using Landis.Extension.SOSIELHarvest.Helpers;
using Landis.SpatialModeling;
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
        public string Name { get { return "LuhyLiteImplementation"; } }

        private ConfigurationModel configuration;
        private AlgorithmModel _algorithmModel;

        private Area[] activeAreas;

        private Dictionary<Area, double> biomass;
        private string _outputFolder;

        /// <summary>
        /// Initializes Luhy lite implementation
        /// </summary> 
        /// <param name="numberOfIterations">Number of internal iterations</param>
        /// <param name="configuration">Parsed agent configuration</param>
        /// <param name="areas">Enumerable of active areas from Landis</param>
        /// <param name="biomass">Active area biomass values which are updated each iteration</param>
        public SosielHarvestImplementation(int numberOfIterations,
            ConfigurationModel configuration,
            IEnumerable<Area> areas,
            Dictionary<Area, double> biomass)
            : base(numberOfIterations,
                  ProcessesConfiguration.GetProcessesConfiguration(configuration.AlgorithmConfiguration.CognitiveLevel)
            )
        {
            this.configuration = configuration;

            this.activeAreas = areas.ToArray();

            this.biomass = biomass;
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

            fmAgents.ForEach(agent =>
            {
                //agent[AlgorithmVariables.Profit] = 0d;
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


            var fmAgents = agentList.GetArchetypesWithPrefix("FM");

            fmAgents.ForEach(feProt =>
            {
                //feProt[AlgorithmVariables.AverageBiomass] = averageBiomass;
            });
        }

        protected override void BeforeCounterfactualThinking(IAgent agent, Area site)
        {
            base.BeforeCounterfactualThinking(agent, site);

            if (agent.Archetype.NamePrefix == "FM")
            {
                //agent[AlgorithmVariables.Biomass] = biomass[site];
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
                //set value of current site biomass to agent variable. 
                //agent[AlgorithmVariables.Biomass] = biomass[site];

                //drop total profit value 
                //agent[AlgorithmVariables.Profit] = 0;
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
            base.PostIterationCalculations(iteration);

        }


        protected override void AfterInnovation(IAgent agent, Area site, DecisionOption newDecisionOption)
        {
            base.AfterInnovation(agent, site, newDecisionOption);

            if (newDecisionOption == null) return;

            var newDecisionOptionModel = new NewDecisionOptionModel()
            {
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
            //agentList.ActiveAgents.ForEach(agent =>
            //{
            //    AgentState<ActiveSite> agentState = iterations.Last.Value[agent];

            //    if (agent[AlgorithmVariables.AgentType] == "Type1")
            //    {
            //        double averageReductionPercentage = agentState.TakenActions.Values.SelectMany(tal => tal)
            //            .Where(ta => ta.VariableName == AlgorithmVariables.ReductionPercentage).Select(ta => (double)ta.Value).DefaultIfEmpty().Average();

            //        double minReductionPercentage = agentState.TakenActions.Values.SelectMany(tal => tal)
            //            .Where(ta => ta.VariableName == AlgorithmVariables.ReductionPercentage).Select(ta => (double)ta.Value).DefaultIfEmpty().Min();

            //        double maxReductionPercentage = agentState.TakenActions.Values.SelectMany(tal => tal)
            //            .Where(ta => ta.VariableName == AlgorithmVariables.ReductionPercentage).Select(ta => (double)ta.Value).DefaultIfEmpty().Max();

            //        double profit = agent[AlgorithmVariables.Profit];

            //        double averageBiomass = agent[AlgorithmVariables.AverageBiomass];

            //        FEValuesOutput values = new FEValuesOutput()
            //        {
            //            Iteration = iteration,
            //            AverageBiomass = averageBiomass,
            //            AverageReductionPercentage = averageReductionPercentage,
            //            MinReductionPercentage = minReductionPercentage,
            //            MaxReductionPercentage = maxReductionPercentage,
            //            BiomassReduction = profit
            //        };

            //        CSVHelper.AppendTo(string.Format("SOSIELHuman_{0}_values.csv", agent.Id), values);
            //    }

            //    //all agent types 

            //    //save activation rule stat
            //    DecisionOption[] activatedRules = agentState.DecisionOptionsHistories.Values.SelectMany(rh => rh.Activated).Distinct().OrderBy(r=>r.Id).ToArray();

            //    string[] activatedRuleIds = activatedRules.Select(r=>r.Id).ToArray();

            //    string[] notActivatedRules = agent.AssignedDecisionOptions.Select(rule => rule.Id).Except(activatedRuleIds).ToArray();

            //    if (agent[AlgorithmVariables.AgentType] == "Type1")
            //    {
            //        FEDOUsageOutput ruleUsage = new FEDOUsageOutput()
            //        {
            //            Iteration = iteration,
            //            ActivatedDOValues = activatedRules.Select(r => string.IsNullOrEmpty(r.Consequent.VariableValue) ? (string)r.Consequent.Value.ToString() : (string)agent[r.Consequent.VariableValue].ToString()).ToArray(),
            //            ActivatedDO = activatedRuleIds,
            //            TotalNumberOfDO = agent.AssignedDecisionOptions.Count,
            //            NotActivatedDO = notActivatedRules
            //        };

            //        CSVHelper.AppendTo(string.Format("SOSIELHuman_{0}_rules.csv", agent.Id), ruleUsage);
            //    }

            //    if (agent[AlgorithmVariables.AgentType] == "Type2")
            //    {
            //        var details = new HMDOUsageOutput()
            //        {
            //            Iteration = iteration,
            //            Age = agent[AlgorithmVariables.Age],
            //            IsAlive = agent[AlgorithmVariables.IsActive],
            //            Income = agent[AlgorithmVariables.AgentIncome],
            //            Expenses = agent[AlgorithmVariables.AgentExpenses],
            //            Savings = agent[AlgorithmVariables.HouseholdSavings],
            //            TotalNumberOfDO = agent.AssignedDecisionOptions.Count,
            //            ChosenDecisionOption = agentState != null ? string.Join("|", agentState.DecisionOptionsHistories[DefaultSite].Activated.Select(opt => opt.Id)) : string.Empty
            //        };

            //        CSVHelper.AppendTo(_outputFolder + string.Format("SOSIELHuman_{0}_rules.csv", agent.Id), details);
            //    }
            //});
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
            return orderedSites.Where(s=>s.AssignedAgents.Contains(agentName)).ToArray();
        }
    }
}
