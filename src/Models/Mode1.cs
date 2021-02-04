using System;
using System.Collections.Generic;
using System.Linq;
using Landis.Core;
using Landis.Extension.SOSIELHarvest.Algorithm;
using Landis.Extension.SOSIELHarvest.Input;
using Landis.Library.BiomassCohorts;
using Landis.Library.HarvestManagement;
using Landis.SpatialModeling;

namespace Landis.Extension.SOSIELHarvest.Models
{
    public class Mode1 : Mode
    {
        private readonly ICore _core;
        private readonly SheParameters _sheParameters;
        private List<Prescription> _prescriptions;
        private ISiteVar<string> _harvestPrescriptionName;
        private ISiteVar<ISiteCohorts> _siteCohorts;

        public Mode1(ICore core, SheParameters sheParameters)
        {
            _core = core;
            _sheParameters = sheParameters;
        }

        public override void Initialize()
        {
            _prescriptions = _sheParameters.Prescriptions;

            _harvestPrescriptionName = _core.GetSiteVar<string>("Harvest.PrescriptionName");

            _siteCohorts = _core.GetSiteVar<ISiteCohorts>("Succession.BiomassCohorts");

            var maDataSet = new ManagementAreaDataset();

            foreach (var agentToManagementArea in _sheParameters.AgentToManagementAreaList)
            {
                foreach (var managementAreaName in agentToManagementArea.ManagementAreas)
                {
                    if (Areas.ContainsKey(managementAreaName) == false)
                    {
                        var managementArea = new ManagementArea(ushort.Parse(managementAreaName));
                        maDataSet.Add(managementArea);
                        var newArea = new Area {Name = managementAreaName};
                        newArea.Initialize(managementArea);
                        Areas.Add(managementAreaName, newArea);
                    }

                    var area = Areas[managementAreaName];
                    area.AssignedAgents.Add(agentToManagementArea.Agent);
                }
            }

            ManagementAreas.ReadMap(_sheParameters.ManagementAreaFileName, maDataSet);
            Stands.ReadMap(_sheParameters.StandsFileName);
            SiteVars.GetExternalVars();
            foreach (ManagementArea mgmtArea in maDataSet)
                mgmtArea.FinishInitialization();
        }

        public override void Harvest()
        {
            foreach (var agentToManagementArea in _sheParameters.AgentToManagementAreaList)
            {
                foreach (var areaName in agentToManagementArea.ManagementAreas)
                {
                    var area = Areas.First(a => a.Key.Equals(areaName)).Value;
                    var harvestManager =
                        new HarvestManager(area, _prescriptions, _harvestPrescriptionName, _siteCohorts);
                    var harvested = harvestManager.Harvest();
                }
            }
        }

        public override HarvestResults AnalyzeHarvestingResult()
        {
            var results = new HarvestResults();

            foreach (var biomassHarvestArea in Areas.Values)
            {
                var managementAreaName = biomassHarvestArea.ManagementArea.MapCode.ToString();

                results.ManageAreaBiomass[managementAreaName] = 0;
                results.ManageAreaHarvested[managementAreaName] = 0;
                results.ManageAreaMaturityPercent[managementAreaName] = 0;

                double manageAreaMaturityProportion = 0;

                foreach (var stand in biomassHarvestArea.ManagementArea)
                {
                    double standMaturityProportion = 0;

                    foreach (var site in stand)
                    {
                        double siteBiomass = 0;
                        double siteMaturity = 0;
                        double siteMaturityProportion;

                        foreach (var species in _core.Species)
                        {
                            var cohorts = _siteCohorts[site][species];

                            if (cohorts == null)
                                continue;

                            double siteSpeciesMaturity = 0;

                            foreach (var cohort in cohorts)
                            {
                                siteBiomass += cohort.Biomass;

                                if (cohort.Age >= _core.Species[species.Name].Maturity)
                                    siteSpeciesMaturity += cohort.Biomass;
                            }

                            siteMaturity += siteSpeciesMaturity;
                        }

                        siteMaturityProportion = Math.Abs(siteBiomass) < 0.0001 ? 0 : siteMaturity / siteBiomass;
                        standMaturityProportion += siteMaturityProportion;

                        results.ManageAreaBiomass[managementAreaName] += siteBiomass;

                        if (_sheParameters.Mode == 1)
                        {
                            results.ManageAreaHarvested[managementAreaName] += 0;
                        }
                        else if (_sheParameters.Mode == 2)
                        {
                            results.ManageAreaHarvested[managementAreaName] +=
                                BiomassHarvest.SiteVars.BiomassRemoved[site];
                        }
                    }

                    standMaturityProportion /= stand.Count();
                    manageAreaMaturityProportion += standMaturityProportion;
                }

                manageAreaMaturityProportion /= biomassHarvestArea.ManagementArea.StandCount;

                results.ManageAreaBiomass[managementAreaName] =
                    results.ManageAreaBiomass[managementAreaName] / 100 * _core.CellArea;

                results.ManageAreaHarvested[managementAreaName] =
                    results.ManageAreaHarvested[managementAreaName] / 100 * _core.CellArea;

                results.ManageAreaMaturityPercent[managementAreaName] = 100 * manageAreaMaturityProportion;
            }

            return results;
        }
    }
}