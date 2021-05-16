// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

namespace Landis.Extension.SOSIELHarvest.Output
{
    public class FMDOUsageOutput
    {
        public int Iteration { get; set; }

        public string ManagementArea { get; set; }

        public string[] ActivatedDOValues { get; set; }

        public string[] ActivatedDO { get; set; }

        public string[] MatchedDO { get; set; }

        public string MostImportantGoal { get; set; }

        public int TotalNumberOfDO { get; set; }
        
        public double BiomassHarvested { get; set; }
        
        public double ManageAreaMaturityPercent { get; set; }
        public double Biomass { get; set; }
    }
}
