// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

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
