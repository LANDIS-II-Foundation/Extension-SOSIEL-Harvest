using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

using Newtonsoft.Json;

namespace Landis.Extension.SOSIELHuman.Configuration
{
    using Enums;


    /// <summary>
    /// Agent state configuration model. Used to parse section "InitialState.AgentsState".
    /// </summary>
    public class AgentStateConfiguration
    {
        [JsonRequired]
        public string PrototypeOfAgent { get; set; }

        [JsonRequired]
        public int NumberOfAgents { get; set; }

        [JsonRequired]
        public Dictionary<string, dynamic> PrivateVariables { get; set; }

        [JsonRequired]
        public Dictionary<string, Dictionary<string, double>> AnticipatedInfluenceState { get; set; }

        [JsonRequired]
        public string[] AssignedRules { get; set; }

        [JsonRequired]
        public string[] AssignedGoals { get; set; }

        [JsonRequired]
        public string[] ActivatedRulesOnFirstIteration { get; set; }
        

        public Dictionary<string, GoalStateConfiguration> GoalsState { get; set; }

        
        public string[] SocialNetwork { get; set; }
    }
}
