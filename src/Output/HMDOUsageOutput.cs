// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

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
