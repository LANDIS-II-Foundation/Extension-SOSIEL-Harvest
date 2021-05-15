// Copyright (C) 2018-2021 The SOSIEL Foundation. All rights reserved.
// Use of this source code is governed by a license that can be found
// in the LICENSE file located in the repository root directory.

using CsvHelper.Configuration.Attributes;

namespace Landis.Extension.SOSIELHarvest.Models
{
    public class AgentGoalAttribute
    {
        public string Agent { get; set; }

        public string Archetype { get; set; }

        public string Goals { get; set; }

        [Name("Goal focal values")]
        public string GoalFocalValues { get; set; }

        [Name("Goal importance")]
        public string GoalImportance { get; set; }

        [Name("Goal value range")]
        public string GoalValueRange { get; set; }
    }
}
