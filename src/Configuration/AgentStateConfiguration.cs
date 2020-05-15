// Can use classes from the listed namespaces.
using System.Collections.Generic;
using Newtonsoft.Json;

/// The container for classes and other namespaces.
namespace Landis.Extension.SOSIELHuman.Configuration
{
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

        public Dictionary<string, Dictionary<string, string>> AnticipatedInfluenceTransform { get; set; }

        [JsonRequired]
        public string[] AssignedDecisionOptions { get; set; }

        [JsonRequired]
        public string[] AssignedGoals { get; set; }

        [JsonRequired]
        public Dictionary<string, GoalStateConfiguration> GoalsState { get; set; }
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
