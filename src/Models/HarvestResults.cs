/// Name: HarvestResults.cs
/// Description:
/// Authors: Multiple.
/// Copyright: Garry Sotnik, Brooke A. Cassell, Robert M. Scheller.

using System.Collections.Generic;
using Landis.Extension.SOSIELHarvest.Algorithm;
using SOSIEL.Entities;

namespace Landis.Extension.SOSIELHarvest.Models
{
    public class HarvestResults
    {
        public HarvestResults()
        {
            ManageAreaBiomass = new Dictionary<string, double>();
            ManageAreaHarvested = new Dictionary<string, double>();
            ManageAreaMaturityPercent = new Dictionary<string, double>();
        }

        public Dictionary<string, double> ManageAreaBiomass { get; }
        public Dictionary<string, double> ManageAreaHarvested { get; }
        public Dictionary<string, double> ManageAreaMaturityPercent { get; }

        public static string GetKey(int mode, IAgent agent, Area area)
        {
            if (mode == 1)
                return $"{agent.Id}_{area.Name}";

            return area.Name;
        }
    }
}