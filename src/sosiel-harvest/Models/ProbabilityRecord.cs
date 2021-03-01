﻿/// Name: ProbabilityRecord.cs
/// Description:
/// Authors: Multiple.
/// Copyright: Garry Sotnik, Brooke A. Cassell, Robert M. Scheller.

using CsvHelper.Configuration;

namespace Landis.Extension.SOSIELHarvest.Models
{
    /// <summary>
    /// Model for parsing probability table file.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ProbabilityRecord<T>
    {
        public T Value { get; set; }

        public double Probability { get; set; }
    }

    public sealed class ProbabilityRecordMap<T> : ClassMap<ProbabilityRecord<T>>
    {
        public ProbabilityRecordMap()
        {
            Map(m => m.Value).Index(0);
            Map(m => m.Probability).Index(1);
        }
    }

}