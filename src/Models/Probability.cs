// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

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
