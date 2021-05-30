// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

using SOSIEL.Enums;

namespace Landis.Extension.SOSIELHarvest.Models
{
    public class GoalAttribute
    {
        public string AgentArchetype { get; set; }

        public string Name { get; set; }

        public GoalTendency Tendency { get; set; }

        public string ReferenceVariable { get; set; }

        public bool ChangeValueOnPrior { get; set; }

        public bool IsCumulative { get; set; }

        public string FocalValueReferenceVariable { get; set; }
    }
}
