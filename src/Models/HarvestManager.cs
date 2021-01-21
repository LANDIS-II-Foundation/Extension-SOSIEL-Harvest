using System;
using System.Collections.Generic;
using Landis.Extension.SOSIELHarvest.Algorithm;
using Landis.Library.BiomassCohorts;
using Landis.SpatialModeling;

namespace Landis.Extension.SOSIELHarvest.Models
{
    public class HarvestManager
    {
        private readonly Area _area;
        private readonly IEnumerable<Prescription> _prescriptions;
        private readonly ISiteVar<string> _harvestPrescriptionName;
        private readonly ISiteVar<ISiteCohorts> _siteCohorts;

        private bool _isHarvestingFinished;
        private readonly List<ActiveSite> _availableSites;

        public HarvestManager(Area area, IEnumerable<Prescription> prescriptions,
            ISiteVar<string> harvestPrescriptionName, ISiteVar<ISiteCohorts> siteCohorts)
        {
            _area = area;
            _prescriptions = prescriptions;
            _harvestPrescriptionName = harvestPrescriptionName;
            _siteCohorts = siteCohorts;

            _availableSites = area.GetActiveSites();
        }

        public int Harvest()
        {
            if (_isHarvestingFinished)
                throw new InvalidOperationException();

            int harvested = 0;

            foreach (var availableSite in _availableSites)
            {
                foreach (var prescription in _prescriptions)
                {
                    var prescriptionHarvested = 0;
                    var siteCohorts = _siteCohorts[availableSite];
                    if (prescription.CheckSiteToHarvest(siteCohorts))
                    {
                        _harvestPrescriptionName[availableSite] = prescription.Name;
                        prescriptionHarvested += HarvestSite(prescription, availableSite, siteCohorts);
                        if(prescriptionHarvested >= prescription.TargetHarvestSize)
                            continue;
                        harvested += prescriptionHarvested;
                    }
                }
            }

            _isHarvestingFinished = true;
            return harvested;
        }

        private int HarvestSite(Prescription prescription, ActiveSite activeSite, ISiteCohorts siteCohorts)
        {
            int harvested = 0;

            foreach (var siteHarvestingRule in prescription.SiteHarvestingRules)
                harvested += siteCohorts.ReduceOrKillBiomassCohorts(new Disturbance(activeSite, siteHarvestingRule));

            return harvested;
        }
    }
}