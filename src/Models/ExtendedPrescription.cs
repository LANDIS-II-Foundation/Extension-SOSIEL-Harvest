// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

using Landis.Library.HarvestManagement;
using Landis.Utilities;

using HarvestManagement = Landis.Library.HarvestManagement;

namespace Landis.Extension.SOSIELHarvest.Models
{
    public class ExtendedPrescription
    {
        public ExtendedPrescription(
            HarvestManagement.Prescription prescription, ManagementArea managementArea, Percentage harvestAreaPercent,
            Percentage harvestStandsAreaPercent, int startTime, int endTime
        )
        {
            Prescription = prescription;
            ManagementArea = managementArea;
            HarvestAreaPercent = harvestAreaPercent;
            HarvestStandsAreaPercent = harvestStandsAreaPercent;
            StartTime = startTime;
            EndTime = endTime;
        }

        public HarvestManagement.Prescription Prescription { get; }
        public ManagementArea ManagementArea { get; }

        public Percentage HarvestAreaPercent { get; }

        public Percentage HarvestStandsAreaPercent { get; }

        public int StartTime { get; }

        public int EndTime { get; }

        public string Name => Prescription.Name;
    }
}
