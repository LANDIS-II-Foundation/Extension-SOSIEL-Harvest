// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

using System;

namespace Landis.Extension.SOSIELHarvest.Helpers
{
    public static class VariableTypeHelper
    {
        /// <summary>
        /// Converts type name to System.Type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Unknown variable type:" + type</exception>
        public static Type ConvertStringToType(string type)
        {
            switch (type.ToLowerInvariant())
            {
                case "integer": return typeof(int);
                case "number": return typeof(double);
                case "string": return typeof(string);
                default: throw new ArgumentOutOfRangeException("Unknown variable type:" + type);
            }
        }
    }
}
