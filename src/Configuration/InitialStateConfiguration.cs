/// Name: InitialStateConfiguration.cs
/// Description:
/// Authors: Multiple.
/// Last updated: July 10th, 2020.
/// Copyright: Garry Sotnik, Brooke A. Cassell, Robert M. Scheller.

using Newtonsoft.Json;

namespace Landis.Extension.SOSIELHarvest.Configuration
{
    /// <summary>
    /// Initial state configuration model. Used to parse section "InitialState".
    /// </summary>
    public class InitialStateConfiguration
    {
        [JsonRequired]
        public AgentStateConfiguration[] AgentsState { get; set; }
    }
}
