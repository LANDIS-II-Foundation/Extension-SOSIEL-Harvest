/// Name: AgentArchetype.cs
/// Description:
/// Authors: Multiple.
/// Last updated: July 10th, 2020.
/// Copyright: Garry Sotnik, Brooke A. Cassell, Robert M. Scheller.

namespace Landis.Extension.SOSIELHarvest.Models
{
    public class AgentArchetype
    {
        public string ArchetypeName { get; set; }

        public string ArchetypePrefix { get; set; }

        public bool DataSetOriented { get; set; }

        public bool GoalImportanceAdjusting { get; set; }
    }
}
