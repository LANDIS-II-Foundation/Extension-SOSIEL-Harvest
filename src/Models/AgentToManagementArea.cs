// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

using System.Collections.Generic;

namespace Landis.Extension.SOSIELHarvest.Models
{
    public class AgentToManagementArea
    {
        public AgentToManagementArea()
        {
            ManagementAreas = new List<string>();
        }

        public string Agent { get; set; }
        public List<string> ManagementAreas { get; }

        public SiteSelectionMethod SiteSelectionMethod { get; set; }
        public int AgentMode { get; set; }
    }
}
