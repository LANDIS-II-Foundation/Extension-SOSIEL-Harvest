/// Name: Mode.cs
/// Description: Base class for all working modes.
/// Authors: Multiple.
/// Copyright: Garry Sotnik, Brooke A. Cassell, Robert M. Scheller.

using System.Collections.Generic;
using System.Linq;

using Landis.Extension.SOSIELHarvest.Algorithm;
using Landis.Extension.SOSIELHarvest.Input;

using SOSIEL.Entities;

namespace Landis.Extension.SOSIELHarvest.Models
{
    public abstract class Mode
    {
        protected readonly SheParameters sheParameters;

        protected IEnumerable<IAgent> Agents { get; private set; }
        public Dictionary<string, Area> Areas { get; set; }

        public abstract void Initialize();

        public void SetAgents(IEnumerable<IAgent> agents)
        {
            Agents = agents.ToList();
            OnAgentsSet();
        }

        public abstract void Harvest(SosielData sosielData);

        public abstract HarvestResults AnalyzeHarvestingResult();

        protected Mode(SheParameters sheParameters)
        {
            this.sheParameters = sheParameters;
            Areas = new Dictionary<string, Area>();
        }

        protected virtual void OnAgentsSet()
        {
        }
    }
}
