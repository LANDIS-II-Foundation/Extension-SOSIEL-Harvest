using Landis.Utilities;

namespace Landis.Extension.SOSIELHarvest.Models
{
    public class SiteSelectionRule
    {
        public SiteSelectionRule(string speciesName, int minAge, int maxAge, Percentage percentage)
        {
            SpeciesName = speciesName;
            MinAge = minAge;
            MaxAge = maxAge;
            Percentage = percentage;
        }
        
        public string SpeciesName { get; }
        public int MinAge { get; }
        public int MaxAge { get; }
        public Percentage Percentage { get; }
    }
}