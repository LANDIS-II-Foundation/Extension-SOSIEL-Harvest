using System.Collections.Generic;
using Landis.Extension.SOSIELHarvest.Algorithm;

namespace Landis.Extension.SOSIELHarvest.Models
{
    public abstract class Mode
    {
        protected Mode()
        {
            Areas = new Dictionary<string, Area>();
        }

        public Dictionary<string, Area> Areas { get; set; }

        public abstract void Initialize();

        public abstract void Harvest();

        public abstract HarvestResults AnalyzeHarvestingResult();
    }
}