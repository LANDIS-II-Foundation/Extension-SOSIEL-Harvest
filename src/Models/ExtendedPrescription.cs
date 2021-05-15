// Copyright (C) 2018-2021 The SOSIEL Foundation. All rights reserved.
// Use of this source code is governed by a license that can be found
// in the LICENSE file located in the repository root directory.

using Landis.Library.HarvestManagement;
using Landis.Utilities;

using HarvestManagement = Landis.Library.HarvestManagement;

namespace Landis.Extension.SOSIELHarvest.Models
{
    public class ExtendedPrescription
    {
        public ExtendedPrescription(HarvestManagement.Prescription prescription, ManagementArea managementArea, Percentage harvestAreaPercent,
            Percentage harvestStandsAreaPercent, 
            int startTime, int endTime)
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
