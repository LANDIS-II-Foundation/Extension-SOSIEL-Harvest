// Copyright (C) 2018-2021 The SOSIEL Foundation. All rights reserved.
// Use of this source code is governed by a license that can be found
// in the LICENSE file located in the repository root directory.

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
