// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

using Landis.Extension.SOSIELHarvest.Models;
using Landis.Library.HarvestManagement;

namespace Landis.Extension.SOSIELHarvest.Helpers
{
    public static class AppliedPrescriptionExtension
    {
        public static ExtendedPrescription ToExtendedPrescription(
            this AppliedPrescription appliedPrescription, ManagementArea managementArea)
        {
            var prescription = appliedPrescription.Prescription;
            var areaToHarvest = appliedPrescription.PercentageToHarvest;
            var standsToHarvest = appliedPrescription.PercentStandsToHarvest;
            var beginTime = appliedPrescription.BeginTime;
            var endTime = appliedPrescription.EndTime;
            return new ExtendedPrescription(prescription, managementArea, areaToHarvest, standsToHarvest,
                beginTime, endTime);
        }
    }
}
