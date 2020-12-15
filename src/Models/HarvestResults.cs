/// Name: HarvestResults.cs
/// Description:
/// Authors: Multiple.
/// Copyright: Garry Sotnik, Brooke A. Cassell, Robert M. Scheller.

using System.Collections.Generic;

namespace Landis.Extension.SOSIELHarvest.Models
{
    public class HarvestResults
    {
        public HarvestResults()
        {
            ManageAreaBiomass = new Dictionary<string, double>();
            ManageAreaHarvested = new Dictionary<string, double>();
            ManageAreaMaturityProportion = new Dictionary<string, double>();
        }

        public Dictionary<string, double> ManageAreaBiomass { get; }
        public Dictionary<string, double> ManageAreaHarvested { get; }
        public Dictionary<string, double> ManageAreaMaturityProportion { get; }
    }
}