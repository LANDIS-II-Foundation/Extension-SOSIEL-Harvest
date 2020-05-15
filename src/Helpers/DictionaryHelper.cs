// Can use classes from the listed namespaces.
using System.Collections.Generic;

// The container for classes and other namespaces.
namespace Landis.Extension.SOSIELHuman.Helpers
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
/*
Copyright 2020 Garry Sotnik

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/
