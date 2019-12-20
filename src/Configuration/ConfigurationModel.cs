using System.Collections.Generic;
using Newtonsoft.Json;
using SOSIEL.Entities;

namespace Landis.Extension.SOSIELHarvest.Configuration
{
    /// <summary>
    /// Main configuration model.
    /// </summary>
    public class ConfigurationModel
    {
        [JsonRequired]
        public AlgorithmConfiguration AlgorithmConfiguration { get; set; }

        [JsonRequired]
        public Dictionary<string, AgentArchetype> AgentConfiguration { get; set; }

        [JsonRequired]
        public InitialStateConfiguration InitialState { get; set; }
    }
}
