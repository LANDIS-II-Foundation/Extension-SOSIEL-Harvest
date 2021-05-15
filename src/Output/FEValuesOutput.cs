// Copyright (C) 2018-2021 The SOSIEL Foundation. All rights reserved.
// Use of this source code is governed by a license that can be found
// in the LICENSE file located in the repository root directory.

namespace Landis.Extension.SOSIELHarvest.Output
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
