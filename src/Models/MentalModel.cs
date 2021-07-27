// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

namespace Landis.Extension.SOSIELHarvest.Models
{
    public class MentalModel
    {
        public string AgentArchetype { get; set; }

        public string Name { get; set; }

        public bool Modifiable { get; set; }

        public int MaxNumberOfDecisionOptions { get; set; }

        public string DecisionOptionGoalRelationship { get; set; }

        public string AssociatedWithGoals { get; set; }

        public string ConsequentValueRange { get; set; }

        public int ConsequentRound { get; set; }
    }
}
