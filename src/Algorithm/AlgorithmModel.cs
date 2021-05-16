// Copyright (C) 2021 SOSIEL Inc. All rights reserved.
// Use of this source code is governed by a license that can be found
// in the LICENSE file located in the repository root directory.

using System.Collections.Generic;

using Landis.Extension.SOSIELHarvest.Models;

namespace Landis.Extension.SOSIELHarvest.Algorithm
{
    /// <summary>
    /// The model used to pass external data.
    /// </summary>
    public class SosielData
    {
        public List<NewDecisionOptionModel> NewDecisionOptions { get; set; }

        // Management area to selected decision options list.
        public Dictionary<string, List<string>> SelectedDecisions { get; set; }

        public HarvestResults HarvestResults { get; set; }
    }
}
