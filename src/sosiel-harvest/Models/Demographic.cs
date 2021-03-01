﻿/// Name: Demographic.cs
/// Description:
/// Authors: Multiple.
/// Copyright: Garry Sotnik, Brooke A. Cassell, Robert M. Scheller.

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