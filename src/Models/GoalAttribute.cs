// Copyright (C) 2018-2021 The SOSIEL Foundation. All rights reserved.
// Use of this source code is governed by a license that can be found
// in the LICENSE file located in the repository root directory.

namespace Landis.Extension.SOSIELHarvest.Models
{
    public class GoalAttribute
    {
        public string AgentArchetype { get; set; }

        public string Name { get; set; }

        public string GoalTendency { get; set; }

        public string ReferenceVariable { get; set; }

        public bool ChangeValueOnPrior { get; set; }

        public bool IsCumulative { get; set; }
    }
}
