// Copyright (C) 2018-2021 The SOSIEL Foundation. All rights reserved.
// Use of this source code is governed by a license that can be found
// in the LICENSE file located in the repository root directory.

namespace Landis.Extension.SOSIELHarvest.Output
{
    public class HMDOUsageOutput
    {
        public int Iteration { get; set; }

        public int Age { get; set; }

        public bool IsAlive { get; set; }

        public int TotalNumberOfDO { get; set; }

        public double Income { get; set; }

        public double Expenses { get; set; }

        public double Savings { get; set; }

        public string ChosenDecisionOption { get; set; }

    }
}
