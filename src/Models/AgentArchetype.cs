// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

namespace Landis.Extension.SOSIELHarvest.Models
{
    public class AgentArchetype
    {
        public string ArchetypeName { get; set; }

        public string ArchetypePrefix { get; set; }

        public bool DataSetOriented { get; set; }

        public bool GoalImportanceAdjusting { get; set; }
    }
}
