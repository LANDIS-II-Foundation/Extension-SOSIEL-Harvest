/// Name: GoalStateConfiguration.cs
/// Description:
/// Authors: Multiple.
/// Copyright: Garry Sotnik, Brooke A. Cassell, Robert M. Scheller.

using Newtonsoft.Json;

namespace Landis.Extension.SOSIELHarvest.Configuration
{
    /// <summary>
    /// Goal state configuration model. Used to parse section "InitialState.AgentsState.GoalsState".
    /// </summary>
    public class GoalStateConfiguration
    {
        [JsonRequired]
        public double Importance { get; set; }

        [JsonRequired]
        public double Value { get; set; }

        public double FocalValue { get; set; }

        public string FocalValueReference { get; set; }

        public double MinValue { get; set; }

        public double MaxValue { get; set; }

        public string MinValueReference { get; set; }

        public string MaxValueReference { get; set; }

        //public bool Randomness { get; set; }

        //public double RandomFrom { get; set; }

        //public double RandomTo { get; set; }

        //public string BasedOn { get; set; }
    }
}
