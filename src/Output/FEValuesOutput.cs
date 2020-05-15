// Can use classes from the listed namespaces.
using System;
using System.Collections.Generic;
using System.Linq;

// The container for classes and other namespaces.
namespace Landis.Extension.SOSIELHuman.Output
{
    public class FEValuesOutput
    {
        public int Iteration { get; set; }

        //public string Site { get; set; }

        public double AverageBiomass { get; set; }

        public double AverageReductionPercentage { get; set; }

        public double MinReductionPercentage { get; set; }

        public double MaxReductionPercentage { get; set; }

        public double BiomassReduction { get; set; }

        //public double Profit { get; set; }
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
