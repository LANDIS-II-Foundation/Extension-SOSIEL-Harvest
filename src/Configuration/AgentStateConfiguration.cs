// Copyright (C) 2018-2021 The SOSIEL Foundation. All rights reserved.
// Use of this source code is governed by a license that can be found
// in the LICENSE file located in the repository root directory.

using System.Collections.Generic;

using Newtonsoft.Json;

namespace Landis.Extension.SOSIELHarvest.Configuration
{
    /// <summary>
    /// Agent state configuration model. Used to parse section "InitialState.AgentsState".
    /// </summary>
    public class AgentStateConfiguration
    {
        public string Name { get; set; }

        [JsonRequired]
        public string PrototypeOfAgent { get; set; }

        [JsonRequired]
        public int NumberOfAgents { get; set; }

        [JsonRequired]
        public Dictionary<string, dynamic> PrivateVariables { get; set; }

        [JsonRequired]
        public Dictionary<string, Dictionary<string, double>> AnticipatedInfluenceState { get; set; }

        //public Dictionary<string, Dictionary<string, string>> AnticipatedInfluenceTransform { get; set; }

        [JsonRequired]
        public string[] AssignedDecisionOptions { get; set; }

        [JsonRequired]
        public string[] AssignedGoals { get; set; }

        [JsonRequired]
        public Dictionary<string, GoalStateConfiguration> GoalsState { get; set; }

        [JsonRequired]
        public int Mode { get; set; }
    }
}
