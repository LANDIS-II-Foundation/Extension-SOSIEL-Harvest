// Copyright (C) 2018-2021 The SOSIEL Foundation. All rights reserved.
// Use of this source code is governed by a license that can be found
// in the LICENSE file located in the repository root directory.

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
