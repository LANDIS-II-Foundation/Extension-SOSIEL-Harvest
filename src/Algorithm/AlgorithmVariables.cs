/// Name: AlgorithmVariables.cs
/// Description:
/// Authors: Multiple.
/// Last updated: July 10th, 2020.
/// Copyright: Garry Sotnik, Brooke A. Cassell, Robert M. Scheller.

 using SOSIEL.Helpers;

namespace Landis.Extension.SOSIELHarvest.Algorithm
{
    /// <summary>
    /// Contains variable names used in code.
    /// </summary>
    public class AlgorithmVariables : SosielVariables
    {
        public const string ManageAreaHarvested = "ManageAreaHarvested";
        public const string ManageAreaMaturityPercent = "ManageAreaMaturityPercent";
        public const string ManageAreaBiomass = "ManageAreaBiomass";

        public const string Mean = "Mean";
        public const string StdDev = "StdDev";

        public const string Group = "Group";

        public const string TargetHarvestSize = "TargetHarvestSize";
    }
}
