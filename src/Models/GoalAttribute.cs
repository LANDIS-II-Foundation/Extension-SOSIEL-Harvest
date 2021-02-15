/// Name: GoalAttribute.cs
/// Description:
/// Authors: Multiple.
/// Copyright: Garry Sotnik, Brooke A. Cassell, Robert M. Scheller.

namespace Landis.Extension.SOSIELHarvest.Models
{
    public class GoalAttribute
    {
        public string AgentArchetype { get; set; }

        public string Name { get; set; }

        public string GoalTendency { get; set; }

        public string ReferenceVariable { get; set; }

        public bool ChangeValueOnPrior { get; set; }

        public bool IsCumulative { get; set; }
    }
}