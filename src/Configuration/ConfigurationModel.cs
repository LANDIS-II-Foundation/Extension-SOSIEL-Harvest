using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;
using SOSIEL.Entities;

namespace Landis.Extension.SOSIELHuman.Configuration
{
    /// <summary>
    /// Main configuration model.
    /// </summary>
    public class ConfigurationModel
    {
        [JsonRequired]
        public AlgorithmConfiguration AlgorithmConfiguration { get; set; }

        [JsonRequired]
        public Dictionary<string, AgentPrototype> AgentConfiguration { get; set; }

        [JsonRequired]
        public InitialStateConfiguration InitialState { get; set; }
    }
}
