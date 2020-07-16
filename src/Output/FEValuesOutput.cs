/// Name: FEValuesOutput.cs
/// Description:
/// Authors: Multiple.
/// Last updated: July 10th, 2020.
/// Copyright: Garry Sotnik, Brooke A. Cassell, Robert M. Scheller.

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
