/// Name: AgentArchetype.cs
/// Description:
/// Authors: Multiple.
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
