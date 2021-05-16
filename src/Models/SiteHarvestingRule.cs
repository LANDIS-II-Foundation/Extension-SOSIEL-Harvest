// Copyright (C) 2021 SOSIEL Inc. All rights reserved.
// Use of this source code is governed by a license that can be found
// in the LICENSE file located in the repository root directory.

using Landis.Core;
using Landis.Library.BiomassCohorts;
using Landis.SpatialModeling;
using Landis.Utilities;

namespace Landis.Extension.SOSIELHarvest.Models
{
    public class SiteHarvestingRule
    {
        public SiteHarvestingRule(string speciesName, int minAge, int maxAge, Percentage percentage)
        {
            SpeciesName = speciesName;
            MinAge = minAge;
            MaxAge = maxAge;
            Percentage = percentage;
        }

        public string SpeciesName { get; }
        public int MinAge { get; }
        public int MaxAge { get; }
        public Percentage Percentage { get; }
    }

    public struct Disturbance : IDisturbance
    {
        private readonly SiteHarvestingRule _siteHarvestingRule;

        public ExtensionType Type { get; }
        public ActiveSite CurrentSite { get; }

        public Disturbance(ActiveSite currentSite, SiteHarvestingRule siteHarvestingRule)
        {
            Type = new ExtensionType("disturbance:harvest");
            _siteHarvestingRule = siteHarvestingRule;
            CurrentSite = currentSite;
        }

        public int ReduceOrKillMarkedCohort(ICohort cohort)
        {
            if (_siteHarvestingRule.SpeciesName.Equals(cohort.Species.Name) == false)
                return 0;

            return (int) (cohort.Biomass * (double) _siteHarvestingRule.Percentage);
        }
    }
}
