﻿/// Name: ExtendedPrescription.cs
/// Description:
/// Authors: Multiple.
/// Copyright: Garry Sotnik, Brooke A. Cassell, Robert M. Scheller.

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