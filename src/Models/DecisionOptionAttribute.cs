// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

namespace Landis.Extension.SOSIELHarvest.Models
{
    public class DecisionOptionAttribute
    {
        public string DecisionOption { get; set; }

        public int RequiredParticipants { get; set; }

        public string ConsequentVariable { get; set; }

        public string ConsequentValue { get; set; }

        public string ConsequentValueReference { get; set; }

        public string ConsequentValueType { get; set; }
    }
}
