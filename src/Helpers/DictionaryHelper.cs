// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

using System.Collections.Generic;

namespace Landis.Extension.SOSIELHarvest.Helpers
{
    public static class DictionaryHelper
    {
        public static void AddToDictionary<T>(this Dictionary<string, List<T>> dictionary, string key, T value)
        {
            List<T> collection;
            if (dictionary.TryGetValue(key, out collection))
                collection.Add(value);
            else
                dictionary.Add(key, new List<T> { value });
        }
    }
}
