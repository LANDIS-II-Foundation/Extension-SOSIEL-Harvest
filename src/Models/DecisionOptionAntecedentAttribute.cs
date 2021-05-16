// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

namespace Landis.Extension.SOSIELHarvest.Models
{
    public class DecisionOptionAntecedentAttribute
    {
        public string DecisionOption { get; set; }

        public string AntecedentVariable { get; set; }

        public string AntecedentOperator { get; set; }

        public string AntecedentValue { get;set; }

        public string AntecedentReference { get; set; }

        public string AntecedentValueType { get; set; }
    }
}
