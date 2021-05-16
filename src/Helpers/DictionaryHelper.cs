// Copyright (C) 2021 SOSIEL Inc. All rights reserved.
// Use of this source code is governed by a license that can be found
// in the LICENSE file located in the repository root directory.

using System.Collections.Generic;

namespace Landis.Extension.SOSIELHarvest.Helpers
{
    public static class DictionaryHelper
    {
        public static void AddToDictionary<T>(this Dictionary<string, List<T>> dictionary, string key, T value)
        {
            List<T> collection;
            if (dictionary.TryGetValue(key, out collection))
            {
                collection.Add(value);
            }
            else
            {
                collection = new List<T> { value };
                dictionary.Add(key, collection);
            }
        }
    }
}
