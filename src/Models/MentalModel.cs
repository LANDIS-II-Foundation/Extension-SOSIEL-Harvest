namespace Landis.Extension.SOSIELHarvest.Models
{
    public class MentalModel
    {
        public string Name { get; set; }

        public bool Modifiable { get; set; }

        public int MaxNumberOfDesignOptions { get; set; }

        public string DesignOptionGoalRelationship { get; set; }

        public string AssociatedWithGoals { get; set; }

        public string ConsequentValueRange { get; set; }

        public string ConsequentRound { get; set; }
    }
}
