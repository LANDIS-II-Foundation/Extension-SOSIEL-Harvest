// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

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
