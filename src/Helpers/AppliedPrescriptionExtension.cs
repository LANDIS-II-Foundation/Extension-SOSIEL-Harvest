using Landis.Extension.SOSIELHarvest.Models;
using Landis.Library.HarvestManagement;

namespace Landis.Extension.SOSIELHarvest.Helpers
{
    public static class AppliedPrescriptionExtension
    {
        public static ExtendedPrescription ToExtendedPrescription(this AppliedPrescription appliedPrescription)
        {
            var prescription = appliedPrescription.Prescription;
            var areaToHarvest = appliedPrescription.PercentageToHarvest;
            var standsToHarvest = appliedPrescription.PercentStandsToHarvest;
            var beginTime = appliedPrescription.BeginTime;
            var endTime = appliedPrescription.EndTime;

            return new ExtendedPrescription(prescription, areaToHarvest, standsToHarvest,
                beginTime, endTime);
        }
    }
}