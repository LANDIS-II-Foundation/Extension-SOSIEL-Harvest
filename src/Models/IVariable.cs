// Copyright (C) 2021 SOSIEL Inc. All rights reserved.
// Use of this source code is governed by a license that can be found
// in the LICENSE file located in the repository root directory.

namespace Landis.Extension.SOSIELHarvest.Models
{
    public interface IVariable
    {
        string Key { get; }

        string VariableName { get; }

        string VariableType { get; }

        string VariableValue { get;  }
    }
}
