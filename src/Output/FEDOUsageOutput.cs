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
