// Can use classes from the listed namespaces.
using System.Linq;
using Landis.Extension.SOSIELHuman.Models;
using SOSIEL.Entities;
using SOSIEL.Helpers;

/// The container for classes and other namespaces.
namespace Landis.Extension.SOSIELHuman.Helpers
{
    public static class ProbabilityTableParser
    {
        /// <summary>
        /// Parses the specified probability table.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="headerExists"></param>
        /// <returns></returns>
        public static ProbabilityTable<T> Parse<T>(string fileName, bool headerExists)
        {
            var probabilities = CSVHelper.ReadAllRecords<ProbabilityRecord<T>>(fileName, headerExists, typeof(ProbabilityRecordMap<T>));

            return new ProbabilityTable<T>(probabilities.ToDictionary(p => p.Value, p => p.Probability));
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
