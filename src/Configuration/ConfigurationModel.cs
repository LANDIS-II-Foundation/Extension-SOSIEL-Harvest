using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

namespace Landis.Extension.SOSIELHuman.Configuration
{
    using Entities;

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
