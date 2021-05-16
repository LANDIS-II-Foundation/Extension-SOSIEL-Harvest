// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

namespace Landis.Extension.SOSIELHarvest.Algorithm
{
    public class NewDecisionOptionModel
    {
        public string ManagementArea { get; set; }

        public string Name { get; set; }

        public string ConsequentVariable { get; set; }

        public double ConsequentValue { get; set; }

        public string BasedOn { get; set; }
    }
}
