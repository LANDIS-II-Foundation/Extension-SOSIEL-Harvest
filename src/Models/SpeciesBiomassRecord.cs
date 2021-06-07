// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

using System;

namespace Landis.Extension.SOSIELHarvest.Models
{
    public class SpeciesBiomassRecord
    {
        public uint ManagementAreaMapCode { get; private set; }

        public int SiteCount { get; set; }

        public double[] TotalAboveGroundBiomass { get; private set; }

        public double[] AverageAboveGroundBiomass { get; private set; }

        public SpeciesBiomassRecord(uint managementAreaMapCode)
        {
            ManagementAreaMapCode = managementAreaMapCode;
            SiteCount = 0;
            TotalAboveGroundBiomass = new double[PlugIn.ModelCore.Species.Count];
            AverageAboveGroundBiomass = new double[PlugIn.ModelCore.Species.Count];
        }

        public void UpdateAverageAboveGroundBiomass()
        {
            int siteCount = SiteCount;
            if (siteCount > 0)
            {
                for (int i = 0; i < AverageAboveGroundBiomass.Length; ++i)
                    AverageAboveGroundBiomass[i] = TotalAboveGroundBiomass[i] / siteCount;
            }
            else
            {
                Array.Clear(AverageAboveGroundBiomass, 0, AverageAboveGroundBiomass.Length);
            }
        }
    }
}
