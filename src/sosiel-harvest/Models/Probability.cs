/// Name: Probability.cs
/// Description:
/// Authors: Multiple.
/// Copyright: Garry Sotnik, Brooke A. Cassell, Robert M. Scheller.

namespace Landis.Extension.SOSIELHarvest.Models
{
    public class Probability
    {
        public string VariableParameter { get; set; }

        public string VariableType { get; set; }

        public string FileName { get; set; }

        public bool IgnoreFirstLine { get; set; }
    }
}