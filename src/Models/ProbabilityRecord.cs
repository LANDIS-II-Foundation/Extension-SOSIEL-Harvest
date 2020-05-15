// Can use classes from the listed namespaces.
using CsvHelper.Configuration;

// The container for classes and other namespaces.
namespace Landis.Extension.SOSIELHuman.Models
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
