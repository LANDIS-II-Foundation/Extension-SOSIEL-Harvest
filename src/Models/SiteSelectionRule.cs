// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

using Landis.Utilities;

namespace Landis.Extension.SOSIELHarvest.Models
{
    public class SiteSelectionRule
    {
        public SiteSelectionRule(string speciesName, int minAge, int maxAge, Percentage percentage)
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

        public bool CheckSite(Landis.Library.BiomassCohorts.ISiteCohorts siteCohorts)
        {
            foreach (var siteCohort in siteCohorts)
            {
                foreach (var speciesCohort in siteCohort)
                {
                    if (speciesCohort.Species.Name.Equals(SpeciesName) && speciesCohort.Age >= MinAge &&
                        speciesCohort.Age <= MaxAge)
                        return true;
                }
            }
            return false;
        }
    }
}
