// Copyright (C) 2018-2021 The SOSIEL Foundation. All rights reserved.
// Use of this source code is governed by a license that can be found
// in the LICENSE file located in the repository root directory.

namespace Landis.Extension.SOSIELHarvest.Models
{
    public class Demographic
    {
        public bool DemographicChange { get; set; }
        public string DeathProbability { get; set; }
        public string BirthProbability { get; set; }
        public string AdoptionProbability { get; set; }
        public double SexualOrientationRate { get; set; }
        public double HomosexualTypeRate { get; set; }
        public double PairingProbability { get; set; }
        public int PairingAgeMin { get; set; }
        public int PairingAgeMax { get; set; }
        public int YearsBetweenBirths { get; set; }
        public int MinimumAgeForHouseholdHead { get; set; }
        public int MaximumAge { get; set; }
    }
}
