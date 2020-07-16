/// Name: DictionaryHelper.cs
/// Description:
/// Authors: Multiple.
/// Last updated: July 10th, 2020.
/// Copyright: Garry Sotnik, Brooke A. Cassell, Robert M. Scheller.

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
