// Copyright (C) 2018-2021 The SOSIEL Foundation. All rights reserved.
// Use of this source code is governed by a license that can be found
// in the LICENSE file located in the repository root directory.

using System.Collections.Generic;

using Landis.Extension.SOSIELHarvest.Algorithm;

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

        public static string GetKey(int mode, IAgent agent, Area area)
        {
            return (mode == 1 || mode == 3)
                ? $"{agent.Id}_{area.Name}"
                : area.Name;
        }
    }
}
