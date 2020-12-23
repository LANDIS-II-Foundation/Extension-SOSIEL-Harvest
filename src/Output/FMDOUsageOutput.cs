/// Name: FMDOUsageOutput.cs
/// Description:
/// Authors: Multiple.
/// Last updated: July 10th, 2020.
/// Copyright: Garry Sotnik, Brooke A. Cassell, Robert M. Scheller.

namespace Landis.Extension.SOSIELHarvest.Output
{
    public class FMDOUsageOutput
    {
        public int Iteration { get; set; }

        public string ManagementArea { get; set; }

        public string[] ActivatedDOValues { get; set; }

        public string[] ActivatedDO { get; set; }

        public string[] MatchedDO { get; set; }

        public string MostImportantGoal { get; set; }

        public int TotalNumberOfDO { get; set; }
        
        public double BiomassHarvested { get; set; }
        
        public double ManageAreaMaturityProportion { get; set; }
    }
}
