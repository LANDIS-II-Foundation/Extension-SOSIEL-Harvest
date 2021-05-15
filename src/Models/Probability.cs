// Copyright (C) 2018-2021 The SOSIEL Foundation. All rights reserved.
// Use of this source code is governed by a license that can be found
// in the LICENSE file located in the repository root directory.

namespace Landis.Extension.SOSIELHarvest.Models
{
    public class Probability
    {
        public string VariableParameter { get; set; }

        public string VariableType { get; set; }

        public string FileName { get; set; }

        public bool IgnoreFirstLine { get; set; }
    }
}
