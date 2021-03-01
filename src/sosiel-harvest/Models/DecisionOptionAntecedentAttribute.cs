/// Name: DecisionOptionAntecedentAttribute.cs
/// Description:
/// Authors: Multiple.
/// Copyright: Garry Sotnik, Brooke A. Cassell, Robert M. Scheller.


namespace Landis.Extension.SOSIELHarvest.Models
{
    public class DecisionOptionAntecedentAttribute
    {
        public string DecisionOption { get; set; }

        public string AntecedentVariable { get; set; }

        public string AntecedentOperator { get; set; }

        public string AntecedentValue { get;set; }

        public string AntecedentReference { get; set; }

        public string AntecedentValueType { get; set; }
    }
}
