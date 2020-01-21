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
    }
}
