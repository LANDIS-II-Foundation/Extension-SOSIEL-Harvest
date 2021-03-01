/// Name: SosielHarvestImplementation.cs
/// Description: 
/// Authors: Multiple.
/// Copyright: Garry Sotnik, Brooke A. Cassell, Robert M. Scheller.

namespace Landis.Extension.SOSIELHarvest.Output
{
    public class HMDOUsageOutput
    {
        public int Iteration { get; set; }

        public int Age { get; set; }

        public bool IsAlive { get; set; }

        public int TotalNumberOfDO { get; set; }

        public double Income { get; set; }

        public double Expenses { get; set; }

        public double Savings { get; set; }

        public string ChosenDecisionOption { get; set; }

    }
}
