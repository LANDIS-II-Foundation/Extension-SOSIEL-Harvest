using System.Collections.ObjectModel;

namespace Landis.Extension.SOSIELHarvest.Models
{
    public class Prescription
    {
        private readonly Collection<SiteSelectionRule> _siteSelectionRules;
        private readonly Collection<SiteHarvestingRule> _siteHarvestingRules;

        public Prescription(string name)
        {
            Name = name;
            _siteSelectionRules = new Collection<SiteSelectionRule>();
            SiteSelectionRules = new ReadOnlyCollection<SiteSelectionRule>(_siteSelectionRules);
            _siteHarvestingRules = new Collection<SiteHarvestingRule>();
            SiteHarvestingRules = new ReadOnlyCollection<SiteHarvestingRule>(_siteHarvestingRules);
        }

        public string Name { get; }

        public float TargetHarvestSize { get; set; }

        public ReadOnlyCollection<SiteSelectionRule> SiteSelectionRules { get; }

        public ReadOnlyCollection<SiteHarvestingRule> SiteHarvestingRules { get; }

        public void AddSiteSelectionRule(SiteSelectionRule siteSelectionRule)
        {
            _siteSelectionRules.Add(siteSelectionRule);
        }

        public void AddSiteHarvestingRule(SiteHarvestingRule siteHarvestingRule)
        {
            _siteHarvestingRules.Add(siteHarvestingRule);
        }
    }
}