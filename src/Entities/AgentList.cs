using System;
using System.Collections.Generic;
using System.Linq;


namespace Landis.Extension.SOSIELHuman.Entities
{
    using Configuration;
    using Exceptions;
    using Randoms;
    using Helpers;
    using Enums;


    public class AgentList
    {
        public List<IAgent> Agents { get; private set; }

        public List<AgentPrototype> Prototypes { get; private set; }

        public IAgent[] ActiveAgents
        {
            get
            {
                return Agents.Where(a => a[VariablesUsedInCode.IsActive] == true).ToArray();
            }
        }

        public AgentList()
        {
            Agents = new List<IAgent>();
            Prototypes = new List<AgentPrototype>();
        }


        /// <summary>
        /// Searches for prototypes with following prefix 
        /// </summary>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public IEnumerable<AgentPrototype> GetPrototypesWithPrefix(string prefix)
        {
            return Prototypes.Where(prototype => prototype.NamePrefix == prefix);
        }

        /// <summary>
        /// Searches for agents with following prefix 
        /// </summary>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public IEnumerable<IAgent> GetAgentsWithPrefix(string prefix)
        {
            return ActiveAgents.Where(agent => agent.Prototype.NamePrefix == prefix);
        }

        /// <summary>
        /// Factory method for initializing agents
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public virtual void Initialize(ConfigurationModel configuration)
        {
            Agents = new List<IAgent>();

            Dictionary<string, AgentPrototype> agentPrototypes = configuration.AgentConfiguration;

            if (agentPrototypes.Count == 0)
            {
                throw new SosielAlgorithmException("Agent prototypes were not defined. See configuration file");
            }


            InitialStateConfiguration initialState = configuration.InitialState;

            //add donothing rule if necessary
            agentPrototypes.ForEach(kvp =>
            {
                AgentPrototype prototype = kvp.Value;

                string prototypeName = kvp.Key;

                if (prototype.MentalProto.Any(set=> set.Layers.Any(layer=> layer.LayerConfiguration.UseDoNothing)))
                {
                    var added = prototype.AddDoNothingRules();

                    initialState.AgentsState.Where(aState => aState.PrototypeOfAgent == prototypeName).ForEach(aState =>
                    {
                        aState.AssignedRules = aState.AssignedRules.Concat(added).ToArray();
                    });
                }
            });

            //save prototypes to list
            Prototypes = new List<AgentPrototype>(agentPrototypes.Values);


            //create agents, groupby is used for saving agents numeration, e.g. FE1, HM1. HM2 etc
            initialState.AgentsState.GroupBy(state => state.PrototypeOfAgent).ForEach((agentStateGroup) =>
            {
                AgentPrototype prototype = agentPrototypes[agentStateGroup.Key];

                int index = 1;

                Dictionary<string, List<Agent>> networks = agentStateGroup.SelectMany(state => state.SocialNetwork ?? new string[0]).Distinct()
                    .ToDictionary(network => network, network => new List<Agent>());

                agentStateGroup.ForEach((agentState) =>
                {
                    Agent agent = Agent.CreateAgent(agentState, prototype);

                    for (int i = 0; i < agentState.NumberOfAgents; i++)
                    {
                        Agent newAgent = agent.Clone();

                        agent.SetId(index);

                        Agents.Add(agent);

                        //check social network and add to temp dictionary
                        if (agentState.SocialNetwork != null)
                        {
                            //set first network to agent variables as household 
                            agent[VariablesUsedInCode.Household] = agentState.SocialNetwork.First();
                            
                            agentState.SocialNetwork.ForEach((network) => networks[network].Add(agent));
                        }

                        index++;
                    }
                });


                //convert temp networks to list of connetcted agents
                networks.ForEach(kvp =>
                {
                    List<Agent> agents = kvp.Value;

                    agents.ForEach(agent =>
                    {
                        agent.ConnectedAgents.AddRange(agents.Where(a => a != agent).Cast<IAgent>());
                    });

                });
            });
        }

    }
}
