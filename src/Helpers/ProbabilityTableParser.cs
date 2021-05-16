// Copyright (C) 2021 SOSIEL Inc. All rights reserved.
// Use of this source code is governed by a license that can be found
// in the LICENSE file located in the repository root directory.

using System.Linq;

using Landis.Extension.SOSIELHarvest.Models;

using SOSIEL.Entities;
using SOSIEL.Helpers;

namespace Landis.Extension.SOSIELHarvest.Helpers
{
    public static class ProbabilityTableParser
    {
        /// <summary>
        /// Parses the specified probability table.
        /// </summary>
        /// <typeparam name="T">Probability table key type</typeparam>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="hasHeader">Indicates that probability file has a header line</param>
        /// <returns>Parsed probability table.</returns>
        public static ProbabilityTable<T> Parse<T>(string fileName, bool hasHeader)
        {
            var probabilities = CSVHelper.ReadAllRecords<ProbabilityRecord<T>>(
                fileName, hasHeader, typeof(ProbabilityRecordMap<T>));
            return new ProbabilityTable<T>(probabilities.ToDictionary(p => p.Value, p => p.Probability));
        }
    }
}
