// Copyright (C) 2021 SOSIEL Inc. All rights reserved.
// Use of this source code is governed by a license that can be found
// in the LICENSE file located in the repository root directory.

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
