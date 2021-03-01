/// Name: ProbabilityTableParser.cs
/// Description:
/// Authors: Multiple.
/// Copyright: Garry Sotnik, Brooke A. Cassell, Robert M. Scheller.

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
