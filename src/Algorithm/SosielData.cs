// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

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
