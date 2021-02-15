using System.Collections.Generic;
using System.Linq;
using Landis.Core;
using Landis.Extension.SOSIELHarvest.Algorithm;
using Landis.Extension.SOSIELHarvest.Input;
using SOSIEL.Entities;

namespace Landis.Extension.SOSIELHarvest.Models
{
    public abstract class Mode
    {
        protected readonly ICore Core;
        protected readonly SheParameters SheParameters;

        protected Mode(ICore core, SheParameters sheParameters)
        {
            Core = core;
            SheParameters = sheParameters;
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

        public abstract void Harvest(SosielData sosielData);

        public abstract HarvestResults AnalyzeHarvestingResult();

        protected virtual void OnAgentsSet()
        {
            
        }
    }
}