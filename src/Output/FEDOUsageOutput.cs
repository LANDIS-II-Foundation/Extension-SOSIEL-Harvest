// Copyright (C) 2021 SOSIEL Inc. All rights reserved.
// Use of this source code is governed by a license that can be found
// in the LICENSE file located in the repository root directory.

namespace Landis.Extension.SOSIELHarvest.Output
{
    public class FEDOUsageOutput
    {
        public int Iteration { get; set; }

        public string[] ActivatedDOValues { get; set; }

        public string[] ActivatedDO { get; set; }

        public int TotalNumberOfDO { get; set; }

        public string[] NotActivatedDO { get; set; }
    }
}
