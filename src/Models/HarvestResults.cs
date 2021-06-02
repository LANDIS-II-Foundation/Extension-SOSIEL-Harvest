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
            ManageAreaBiomass = new Dictionary<string, double>();
            ManageAreaHarvested = new Dictionary<string, double>();
            ManageAreaMaturityPercent = new Dictionary<string, double>();
        }

        public Dictionary<string, double> ManageAreaBiomass { get; }
        public Dictionary<string, double> ManageAreaHarvested { get; }
        public Dictionary<string, double> ManageAreaMaturityPercent { get; }

        public static string GetKey(int mode, IAgent agent, IDataSet area)
        {
            return (mode == 1 || mode == 3) ? $"{agent.Id}_{area.Name}" : area.Name;
        }
    }
}
