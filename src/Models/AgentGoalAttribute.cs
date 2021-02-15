/// Name: AgentGoalAttribute.cs
/// Description:
/// Authors: Multiple.
/// Copyright: Garry Sotnik, Brooke A. Cassell, Robert M. Scheller.

using CsvHelper.Configuration.Attributes;

namespace Landis.Extension.SOSIELHarvest.Models
{
    public class AgentGoalAttribute
    {
        public string Agent { get; set; }

        public string Archetype { get; set; }

        public string Goals { get; set; }

        [Name("Goal focal values")]
        public string GoalFocalValues { get; set; }

        [Name("Goal importance")]
        public string GoalImportance { get; set; }

        [Name("Goal value range")]
        public string GoalValueRange { get; set; }
    }
}
