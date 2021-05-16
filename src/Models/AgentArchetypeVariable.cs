// Copyright (C) 2021 SOSIEL Inc. All rights reserved.
// Use of this source code is governed by a license that can be found
// in the LICENSE file located in the repository root directory.

namespace Landis.Extension.SOSIELHarvest.Models
{
    public class AgentArchetypeVariable: IVariable
    {
        public string Key => ArchetypeName;

        public string ArchetypeName { get; set; }

        public string VariableName { get; set; }

        public string VariableType { get; set; }

        public string VariableValue { get; set; }
    }
}
