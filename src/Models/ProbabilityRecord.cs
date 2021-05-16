// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

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
