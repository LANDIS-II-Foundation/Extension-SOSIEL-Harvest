using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Landis.Extension.SOSIELHarvest.Algorithm;
using SOSIEL.Entities;

namespace Landis.Extension.SOSIELHarvest.Models
{
    public abstract class Mode
    {
        protected Mode()
        {
            Areas = new Dictionary<string, Area>();
        }

        public Dictionary<string, Area> Areas { get; set; }

        protected IEnumerable<IAgent> Agents { get; private set; }

        public abstract void Initialize();

        public void SetAgents(IEnumerable<IAgent> agents)
        {
            Agents = agents.ToList();
            OnAgentsSet();
        }

        public abstract void Harvest();

        public abstract HarvestResults AnalyzeHarvestingResult();

        protected virtual void OnAgentsSet()
        {
            
        }
    }
}