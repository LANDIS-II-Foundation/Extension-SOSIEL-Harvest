// Copyright (C) 2018-2021 The SOSIEL Foundation. All rights reserved.
// Use of this source code is governed by a license that can be found
// in the LICENSE file located in the repository root directory.

using System.Collections.ObjectModel;

using Landis.Library.BiomassCohorts;

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

        public bool CheckSiteToHarvest(ISiteCohorts siteCohorts)
        {
            foreach (var siteSelectionRule in SiteSelectionRules)
            {
                var result = siteSelectionRule.CheckSite(siteCohorts);
                if (result == false)
                    return false;
            }

            return true;
        }

        public Prescription Copy(string name, float targetHarvestSize)
        {
            var copy = new Prescription(name) {TargetHarvestSize = targetHarvestSize};

            foreach (var siteSelectionRule in _siteSelectionRules)
                copy.AddSiteSelectionRule(siteSelectionRule);

            foreach (var siteHarvestingRule in _siteHarvestingRules)
                copy.AddSiteHarvestingRule(siteHarvestingRule);

            return copy;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
