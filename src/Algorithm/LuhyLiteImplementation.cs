/// Name: LuhyLiteImplementation.cs
/// Description: 
/// Authors: Multiple.
/// Last updated: July 10th, 2020.
/// Copyright: Garry Sotnik, Brooke A. Cassell, Robert M. Scheller.

// Can use classes from the listed namespaces.
using System;
using System.Collections.Generic;
using System.Linq;
using Landis.Extension.SOSIELHuman.Helpers;
using Landis.Library.BiomassCohorts;
using Landis.SpatialModeling;
using SOSIEL.Algorithm;
using SOSIEL.Configuration;
using SOSIEL.Entities;
using SOSIEL.Exceptions;
using SOSIEL.Helpers;
using SOSIEL.Processes;

// The container for classes and other namespaces.
namespace Landis.Extension.SOSIELHuman.Algorithm
{
    using Configuration;
    using Output;
    // The container for data and methods.
    public class LuhyLiteImplementation : SosielAlgorithm<ActiveSite>, IAlgorithm<AlgorithmModel>
    {
        public string Name { get { return "LuhyLiteImplementation"; } }

        private ConfigurationModel configuration;

        private ActiveSite[] activeSites;

        private Dictionary<ActiveSite, double> biomass;
        private string _outputFolder;

        /// <summary>
        /// Initializes Luhy lite implementation
        /// </summary>
        /// <param name="numberOfIterations">Number of internal iterations</param>
        /// <param name="configuration">Parsed agent configuration</param>
        /// <param name="activeSites">Enumerable of active sites from Landis</param>
        /// <param name="biomass">Active site biomass values which are updated each iteration</param>
        public LuhyLiteImplementation(int numberOfIterations,
            ConfigurationModel configuration,
            IEnumerable<ActiveSite> activeSites,
            Dictionary<ActiveSite, double> biomass)
            : base(numberOfIterations,
                  ProcessesConfiguration.GetProcessesConfiguration(configuration.AlgorithmConfiguration.CognitiveLevel)
            )
        {
            this.configuration = configuration;

            this.activeSites = activeSites.ToArray();

            this.biomass = biomass;
        }

        /// <summary>
        /// Executes agent initializing. It's the first initializing step.
        /// </summary>
        protected override void InitializeAgents()
        {
            var agents = new List<IAgent>();

            Dictionary<string, AgentPrototype> agentPrototypes = configuration.AgentConfiguration;

            if (agentPrototypes.Count == 0)
            {
                throw new SosielAlgorithmException("Agent prototypes were not defined. See configuration file");
            }

            InitialStateConfiguration initialState = configuration.InitialState;

            var networks = new Dictionary<string, List<SOSIEL.Entities.Agent>>();

            //create agents, groupby is used for saving agents numeration, e.g. FE1, HM1. HM2 etc
            initialState.AgentsState.GroupBy(state => state.PrototypeOfAgent).ForEach((agentStateGroup) =>
            {
                AgentPrototype prototype = agentPrototypes[agentStateGroup.Key];
                var mentalProto = prototype.MentalProto;
                int index = 1;

                agentStateGroup.ForEach((agentState) =>
                {
                    for (int i = 0; i < agentState.NumberOfAgents; i++)
                    {
                        Agent agent = LuhyAgent.CreateAgent(agentState, prototype);
                        agent.SetId(index);

                        agents.Add(agent);

                        if (prototype.NamePrefix == "HM")
                        {
                            networks.AddToDictionary((string) agent[AlgorithmVariables.Household], agent);
                            networks.AddToDictionary((string) agent[AlgorithmVariables.NuclearFamily], agent);

                            if (agent.ContainsVariable(AlgorithmVariables.ExternalRelations))
                            {
                                var externals = (string) agent[AlgorithmVariables.ExternalRelations];

                                foreach (var en in externals.Split(';'))
                                {
                                    networks.AddToDictionary(en, agent);
                                }
                            }

                            //household and extended family are the same at the beginning
                            agent[AlgorithmVariables.ExtendedFamily] = new List<string>()
                                {(string) agent[AlgorithmVariables.Household]};
                        }

                        index++;
                    }
                });
            });

            //convert temp networks to list of connetcted agents
            networks.ForEach(kvp =>
            {
                var connectedAgents = kvp.Value;

                connectedAgents.ForEach(agent =>
                {
                    agent.ConnectedAgents.AddRange(connectedAgents.Where(a => a != agent).Except(agent.ConnectedAgents));
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

            demographic = new Demographic<ActiveSite>(configuration.AlgorithmConfiguration.DemographicConfiguration,
                probabilities.GetProbabilityTable<int>(AlgorithmProbabilityTables.BirthProbabilityTable),
                probabilities.GetProbabilityTable<int>(AlgorithmProbabilityTables.DeathProbabilityTable));
        }

        /// <summary>
        /// Executes iteration state initializing. Executed after InitializeAgents.
        /// </summary>
        /// <returns></returns>
        ///
        protected override Dictionary<IAgent, AgentState<ActiveSite>> InitializeFirstIterationState()
        {
            var states = new Dictionary<IAgent, AgentState<ActiveSite>>();

            agentList.Agents.ForEach(agent =>
            {
                //creates empty agent state
                AgentState<ActiveSite> agentState = AgentState<ActiveSite>.Create(agent.Prototype.IsSiteOriented);

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
            InitializeAgents();

            InitializeProbabilities();

            if (configuration.AlgorithmConfiguration.UseDimographicProcesses)
            {
                UseDemographic();
            }

            //InitializeFirstIterationState();

            AfterInitialization();
        }



        /// <summary>
        /// Runs as many internal iterations as passed to the constructor
        /// </summary>
        public AlgorithmModel Run(AlgorithmModel data)
        {
            RunSosiel(activeSites);

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
            var feAgents = agentList.GetAgentsWithPrefix("FE");

            feAgents.ForEach(agent =>
            {
                agent[AlgorithmVariables.Profit] = 0d;
            });



            var hmAgents = agentList.GetAgentsWithPrefix("HM");

            hmAgents.ForEach(agent =>
            {

                agent[AlgorithmVariables.AgentIncome] = 0d;
                agent[AlgorithmVariables.AgentExpenses] = 0d;
                agent[AlgorithmVariables.AgentSavings] = 0d;
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


            //calculate biomass for each site
            foreach(ActiveSite site in activeSites)
            {
                biomass[site] = ComputeLivingBiomass(site);
            }



            //----
            //calculate tourism value
            double averageBiomass = biomass.Values.Average();

            var fePrototypes = agentList.GetPrototypesWithPrefix("FE");

            fePrototypes.ForEach(feProt =>
            {
                feProt[AlgorithmVariables.AverageBiomass] = averageBiomass;
            });

            var hmPrototypes = agentList.GetPrototypesWithPrefix("HM");

            hmPrototypes.ForEach(hmProt =>
            {
                hmProt[AlgorithmVariables.Tourism] = averageBiomass >= hmProt[AlgorithmVariables.TourismThreshold];
            });
        }

        protected override void BeforeCounterfactualThinking(IAgent agent, ActiveSite site)
        {
            base.BeforeCounterfactualThinking(agent, site);

            if (agent[AlgorithmVariables.AgentType] == "Type1")
            {
                //set value of current site biomass to agent variable.
                agent[AlgorithmVariables.Biomass] = biomass[site];
            }
        }


        /// <summary>
        /// Executes before action selection process
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="site"></param>
        protected override void BeforeActionSelection(IAgent agent, ActiveSite site)
        {
            //call default implementation
            base.BeforeActionSelection(agent, site);

            //if agent is FE, set to local variables current site biomass
            if (agent[AlgorithmVariables.AgentType] == "Type1")
            {
                //set value of current site biomass to agent variable.
                agent[AlgorithmVariables.Biomass] = biomass[site];


                //drop total profit value
                agent[AlgorithmVariables.Profit] = 0;
            }
        }

        /// <summary>
        /// Executes after action taking process
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="site"></param>
        protected override void AfterActionTaking(IAgent agent, ActiveSite site)
        {
            //call default implementation
            base.AfterActionTaking(agent, site);


            if (agent[AlgorithmVariables.AgentType] == "Type1")
            {
                double reductionPercent = agent[AlgorithmVariables.ReductionPercentage] / 100d;

                //compute profit
                double profit = ReduceBiomass(site, reductionPercent);
                //add computed profit to total profit
                agent[AlgorithmVariables.Profit] += profit;

                //reduce biomass
                biomass[site] -= profit;
            }

        }

        protected override void PostIterationCalculations(int iteration)
        {
            base.PostIterationCalculations(iteration);

            //----
            //calculate household values (income, expenses, savings) for each agent in specific household
            var hmAgents = agentList.GetAgentsWithPrefix("HM");

            hmAgents.GroupBy(agent => agent[SosielVariables.Household])
                .ForEach(householdAgents =>
                {
                    double householdIncome =
                        householdAgents.Sum(agent => (double)agent[AlgorithmVariables.AgentIncome]);
                    double householdExpenses =
                        householdAgents.Sum(agent => (double)agent[AlgorithmVariables.AgentExpenses]);
                    double iterationHouseholdSavings = householdIncome - householdExpenses;
                    double householdSavings = householdAgents.Where(agent => agent.ContainsVariable(AlgorithmVariables.HouseholdSavings))
                                                  .Select(agent => (double)agent[AlgorithmVariables.HouseholdSavings]).FirstOrDefault() + iterationHouseholdSavings;

                    householdAgents.ForEach(agent =>
                    {
                        agent[AlgorithmVariables.HouseholdIncome] = householdIncome;
                        agent[AlgorithmVariables.HouseholdExpenses] = householdExpenses;
                        agent[AlgorithmVariables.HouseholdSavings] = householdSavings;
                    });
                });
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
                AgentState<ActiveSite> agentState = iterations.Last.Value[agent];

                if (agent[AlgorithmVariables.AgentType] == "Type1")
                {
                    double averageReductionPercentage = agentState.TakenActions.Values.SelectMany(tal => tal)
                        .Where(ta => ta.VariableName == AlgorithmVariables.ReductionPercentage).Select(ta => (double)ta.Value).DefaultIfEmpty().Average();

                    double minReductionPercentage = agentState.TakenActions.Values.SelectMany(tal => tal)
                        .Where(ta => ta.VariableName == AlgorithmVariables.ReductionPercentage).Select(ta => (double)ta.Value).DefaultIfEmpty().Min();

                    double maxReductionPercentage = agentState.TakenActions.Values.SelectMany(tal => tal)
                        .Where(ta => ta.VariableName == AlgorithmVariables.ReductionPercentage).Select(ta => (double)ta.Value).DefaultIfEmpty().Max();

                    double profit = agent[AlgorithmVariables.Profit];

                    double averageBiomass = agent[AlgorithmVariables.AverageBiomass];

                    FEValuesOutput values = new FEValuesOutput()
                    {
                        Iteration = iteration,
                        AverageBiomass = averageBiomass,
                        AverageReductionPercentage = averageReductionPercentage,
                        MinReductionPercentage = minReductionPercentage,
                        MaxReductionPercentage = maxReductionPercentage,
                        BiomassReduction = profit
                    };

                    CSVHelper.AppendTo(string.Format("SOSIELHuman_{0}_values.csv", agent.Id), values);
                }

                //all agent types

                //save activation rule stat
                DecisionOption[] activatedRules = agentState.DecisionOptionsHistories.Values.SelectMany(rh => rh.Activated).Distinct().OrderBy(r=>r.Id).ToArray();

                string[] activatedRuleIds = activatedRules.Select(r=>r.Id).ToArray();

                string[] notActivatedRules = agent.AssignedDecisionOptions.Select(rule => rule.Id).Except(activatedRuleIds).ToArray();

                if (agent[AlgorithmVariables.AgentType] == "Type1")
                {
                    FEDOUsageOutput ruleUsage = new FEDOUsageOutput()
                    {
                        Iteration = iteration,
                        ActivatedDOValues = activatedRules.Select(r => string.IsNullOrEmpty(r.Consequent.VariableValue) ? (string)r.Consequent.Value.ToString() : (string)agent[r.Consequent.VariableValue].ToString()).ToArray(),
                        ActivatedDO = activatedRuleIds,
                        TotalNumberOfDO = agent.AssignedDecisionOptions.Count,
                        NotActivatedDO = notActivatedRules
                    };

                    CSVHelper.AppendTo(string.Format("SOSIELHuman_{0}_rules.csv", agent.Id), ruleUsage);
                }

                if (agent[AlgorithmVariables.AgentType] == "Type2")
                {
                    var details = new HMDOUsageOutput()
                    {
                        Iteration = iteration,
                        Age = agent[AlgorithmVariables.Age],
                        IsAlive = agent[AlgorithmVariables.IsActive],
                        Income = agent[AlgorithmVariables.AgentIncome],
                        Expenses = agent[AlgorithmVariables.AgentExpenses],
                        Savings = agent[AlgorithmVariables.HouseholdSavings],
                        TotalNumberOfDO = agent.AssignedDecisionOptions.Count,
                        ChosenDecisionOption = agentState != null ? string.Join("|", agentState.DecisionOptionsHistories[DefaultSite].Activated.Select(opt => opt.Id)) : string.Empty
                    };

                    CSVHelper.AppendTo(_outputFolder + string.Format("SOSIELHuman_{0}_rules.csv", agent.Id), details);
                }
            });
        }

        protected override void Maintenance()
        {
            base.Maintenance();

            //clean up unassigned rules
            agentList.Prototypes.ForEach(prototype =>
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


            var hmAgents = agentList.GetAgentsWithPrefix("HM");

            hmAgents.ForEach(agent =>
            {
                //increase household members age

                if ((bool)agent[AlgorithmVariables.IsActive])
                {
                    agent[AlgorithmVariables.Age] += 1;
                }
                else
                {
                    agent[AlgorithmVariables.AgentIncome] = 0;
                    agent[AlgorithmVariables.AgentExpenses] = 0;
                    agent[AlgorithmVariables.HouseholdSavings] = 0;
                }
            });

        }

        private double ComputeLivingBiomass(ActiveSite site)
        {
            int total = 0;

            ISiteCohorts siteCohorts = SiteVars.Cohorts[site];

            if (siteCohorts != null)
                foreach (ISpeciesCohorts speciesCohorts in siteCohorts)
                    foreach (ICohort cohort in speciesCohorts)
                        total += (int)(cohort.Biomass);
            //total += ComputeBiomass(speciesCohorts);
            return total;
        }


        /// <summary>
        /// Reduces biomass
        /// </summary>
        /// <param name="site"></param>
        /// <param name="reductionPercent"></param>
        /// <returns>Returns biomass reduction</returns>
        private double ReduceBiomass(ActiveSite site, double reductionPercent)
        {
            double initialBiomass = biomass[site];

            //PlugIn.ModelCore.UI.WriteLine("   Percent Reduction = {0}", reductionPercent);
            PartialDisturbance.ReduceCohortBiomass(site, reductionPercent);

            //calculate new biomass value
            double reducedBiomass = ComputeLivingBiomass(site);

            return initialBiomass - reducedBiomass;
        }
    }
}
/*
Copyright 2020 Garry Sotnik

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/
