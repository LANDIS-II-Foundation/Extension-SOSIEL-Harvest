// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

using System.Collections.Generic;

using SOSIEL.Entities;

namespace Landis.Extension.SOSIELHarvest.Models
{
    public class HarvestResults
    {
        public HarvestResults()
        {
            ManagementAreaBiomass = new Dictionary<string, double>();
            ManagementAreaHarvested = new Dictionary<string, double>();
            ManagementAreaMaturityPercent = new Dictionary<string, double>();
        }

        public Dictionary<string, double> ManagementAreaBiomass { get; }
        public Dictionary<string, double> ManagementAreaHarvested { get; }
        public Dictionary<string, double> ManagementAreaMaturityPercent { get; }

        public static string GetKey(int mode, IAgent agent, IDataSet area)
        {
            return (mode == 1 || mode == 3) ? $"{agent.Id}_{area.Name}" : area.Name;
        }
    }
}
