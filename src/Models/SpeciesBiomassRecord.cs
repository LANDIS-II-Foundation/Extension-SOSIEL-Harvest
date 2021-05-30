// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

using Landis.Core;

namespace Landis.Extension.SOSIELHarvest.Models
{
    public class SpeciesBiomassRecord
    {
        public IEcoregion EcoRegion { get; set; }
        public ISpecies Species { get; set; }
        public int SiteCount { get; set; }
        public double AverageAboveGroundBiomass { get; set; }
    }
}
