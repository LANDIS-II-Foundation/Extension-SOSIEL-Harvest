// Copyright (C) 2021 SOSIEL Inc. All rights reserved.
// Use of this source code is governed by a license that can be found
// in the LICENSE file located in the repository root directory.

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
