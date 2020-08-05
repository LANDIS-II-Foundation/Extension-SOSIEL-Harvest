/// Name: FEDOUsageOutput.cs
/// Description:
/// Authors: Multiple.
/// Last updated: July 10th, 2020.
/// Copyright: Garry Sotnik, Brooke A. Cassell, Robert M. Scheller.

// Can use classes from the listed namespaces.



// The container for classes and other namespaces.
namespace Landis.Extension.SOSIELHarvest.Output
{
    public class FEDOUsageOutput
    {
        public int Iteration { get; set; }

        public string[] ActivatedDOValues { get; set; }

        public string[] ActivatedDO { get; set; }

        public int TotalNumberOfDO { get; set; }

        public string[] NotActivatedDO { get; set; }
    }
}
