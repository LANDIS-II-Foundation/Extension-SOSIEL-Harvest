using Landis.Library.HarvestManagement;
using Landis.Utilities;

namespace Landis.Extension.SOSIELHarvest.Models
{
    public class ExtendedPrescription
    {
        public ExtendedPrescription(Prescription prescription, Percentage harvestAreaPercent,
            Percentage harvestStandsAreaPercent,
            int startTime, int endTime)
        {
            Prescription = prescription;
            HarvestAreaPercent = harvestAreaPercent;
            HarvestStandsAreaPercent = harvestStandsAreaPercent;
            StartTime = startTime;
            EndTime = endTime;
        }

        public Prescription Prescription { get; }

        public Percentage HarvestAreaPercent { get; }

        public Percentage HarvestStandsAreaPercent { get; }

        public int StartTime { get; }

        public int EndTime { get; }
    }
}