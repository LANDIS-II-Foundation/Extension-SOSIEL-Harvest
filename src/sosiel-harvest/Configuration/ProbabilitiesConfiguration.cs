/// Name: ProbabilitiesConfiguration.cs
/// Description:
/// Authors: Multiple.
/// Copyright: Garry Sotnik, Brooke A. Cassell, Robert M. Scheller.

using Newtonsoft.Json;

namespace Landis.Extension.SOSIELHarvest.Configuration
{
    /// <summary>
    /// Probabilities configuration model. Used to parse section "ProbabilitiesConfiguration".
    /// </summary>
    public class ProbabilitiesConfiguration
    {
        [JsonRequired]
        public string Variable { get; set; }

        [JsonRequired]
        public string FilePath { get; set; }

        [JsonRequired]
        public string VariableType { get; set; }

        public bool WithHeader { get; set; }
    }
}
