// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Landis.Extension.SOSIELHarvest.Algorithm;
using Landis.Extension.SOSIELHarvest.Input;
using Landis.Library.BiomassCohorts;
using Landis.Library.HarvestManagement;
using Landis.SpatialModeling;

namespace Landis.Extension.SOSIELHarvest.Models
{
    public class Mode1 : Mode
    {
        private List<Prescription> _prescriptions;
        private ISiteVar<string> _harvestPrescriptionName;
        private ISiteVar<ISiteCohorts> _siteCohorts;
        private readonly Dictionary<string, double> _harvested;

        public Mode1(PlugIn plugin)
            : base(1, plugin)
        {
            _harvested = new Dictionary<string, double>();
        }

        protected override void InitializeMode()
        {
            _prescriptions = sheParameters.Prescriptions;
            _harvestPrescriptionName = PlugIn.ModelCore.GetSiteVar<string>("Harvest.PrescriptionName");
            _siteCohorts = PlugIn.ModelCore.GetSiteVar<ISiteCohorts>("Succession.BiomassCohorts");

            var maDataSet = new ManagementAreaDataset();
            foreach (var agentToManagementArea in sheParameters.AgentToManagementAreaList)
            {
                foreach (var managementAreaName in agentToManagementArea.ManagementAreas)
                {
                    var managementArea = new ManagementArea(ushort.Parse(managementAreaName));
                    maDataSet.Add(managementArea);
                }
            }

            ManagementAreas.ReadMap(sheParameters.ManagementAreaFileName, maDataSet);
            Stands.ReadMap(sheParameters.StandsFileName);
            SiteVars.GetExternalVars();

            foreach (ManagementArea mgmtArea in maDataSet)
                mgmtArea.FinishInitialization();

            foreach (var agentToManagementArea in sheParameters.AgentToManagementAreaList)
            {
                foreach (var managementAreaName in agentToManagementArea.ManagementAreas)
                {
                    Area area;
                    if (!Areas.TryGetValue(managementAreaName, out area))
                    {
                        area = new Area();
                        area.Initialize(maDataSet.First(ma => ma.MapCode.ToString() == managementAreaName));
                        Areas.Add(managementAreaName, area);
                    }
                    area.AssignedAgents.Add(agentToManagementArea.Agent);
                }
            }
        }

        protected override void Harvest()
        {
            ClearHarvested();
            GenerateNewPrescriptions(sosielData);
            PlugIn.ModelCore.UI.WriteLine("Run Mode1 harvesting ...");
            foreach (var agent in Agents)
            {
                var areas = sheParameters.AgentToManagementAreaList
                    .First(map => map.Agent.Equals(agent.Id)).ManagementAreas
                    .Select(ma => Areas.First(area => area.Key.Equals(ma)).Value);
                foreach (var area in areas)
                {
                    var key = HarvestResults.GetKey(ModeId, agent, area);
                    //Debugger.Launch();
                    if (sosielData.SelectedDecisions.ContainsKey(key))
                    {
                        var selectedPrescriptionNames = sosielData.SelectedDecisions[key];
                        PlugIn.ModelCore.UI.WriteLine(
                            $"\t\t{key}: decisions: {string.Join(",", selectedPrescriptionNames)}");
                        var selectedPrescriptions = _prescriptions.Where(
                            p => selectedPrescriptionNames.Contains(p.Name));
                        var harvestManager =
                            new HarvestManager(area, selectedPrescriptions, _harvestPrescriptionName, _siteCohorts);
                        _harvested[key] = harvestManager.Harvest();
                        PlugIn.ModelCore.UI.WriteLine(
                            $"\t\t{key}: harvested {harvestManager.HarvestedSitesNumber} sites");
                    }
                    else
                    {
                        PlugIn.ModelCore.UI.WriteLine($"\t\t{key}: harvested 0 sites (no decision found)");
                    }
                }
            }
        }

        protected override HarvestResults AnalyzeHarvestingResult()
        {
            var results = new HarvestResults();
            foreach (var agent in Agents)
            {
                var areas = sheParameters.AgentToManagementAreaList
                    .First(map => map.Agent == agent.Id).ManagementAreas
                    .Select(ma => Areas.First(area => area.Key == ma).Value);
                foreach (var area in areas)
                {
                    var key = HarvestResults.GetKey(1, agent, area);
                    results.ManageAreaBiomass[key] = 0;
                    results.ManageAreaMaturityPercent[key] = 0;

                    double manageAreaMaturityProportion = 0;
                    foreach (var stand in area.ManagementArea)
                    {
                        double standMaturityProportion = 0;
                        foreach (var site in stand)
                        {
                            double siteBiomass = 0;
                            double siteMaturity = 0;

                            foreach (var species in PlugIn.ModelCore.Species)
                            {
                                var cohorts = _siteCohorts[site][species];
                                if (cohorts == null) continue;
                                double siteSpeciesMaturity = 0;
                                foreach (var cohort in cohorts)
                                {
                                    siteBiomass += cohort.Biomass;
                                    if (cohort.Age >= PlugIn.ModelCore.Species[species.Name].Maturity)
                                        siteSpeciesMaturity += cohort.Biomass;
                                }
                                siteMaturity += siteSpeciesMaturity;
                            }

                            var siteMaturityProportion = Math.Abs(siteBiomass) < 0.0001 
                                ? 0 : (siteMaturity / siteBiomass) * 2;
                            results.ManageAreaBiomass[key] += siteBiomass;
                        }
                        standMaturityProportion /= stand.Count();
                        manageAreaMaturityProportion += standMaturityProportion;
                    }

                    manageAreaMaturityProportion /= area.ManagementArea.StandCount;
                    results.ManageAreaBiomass[key] = results.ManageAreaBiomass[key] / 100 * PlugIn.ModelCore.CellArea;
                    results.ManageAreaHarvested[key] = _harvested[key] * PlugIn.ModelCore.CellArea;
                    results.ManageAreaMaturityPercent[key] = 100 * manageAreaMaturityProportion;
                }
            }
            return results;
        }

        protected override void OnAgentsSet()
        {
            base.OnAgentsSet();
            ClearHarvested();
        }

        private void GenerateNewPrescriptions(SosielData sosielData)
        {
            foreach (var newDecisionOption in sosielData.NewDecisionOptions)
            {
                var prescription = _prescriptions.FirstOrDefault(
                    p => p.Name.Equals(newDecisionOption.BasedOn));
                if (prescription != null)
                {
                    _prescriptions.Add(prescription.Copy(newDecisionOption.Name,
                        (float)newDecisionOption.ConsequentValue));
                }
            }
        }

        private void ClearHarvested()
        {
            foreach (var agent in Agents)
            {
                PlugIn.ModelCore.UI.WriteLine($"ClearHarvested: agent {agent.Id}");
                var areas = sheParameters.AgentToManagementAreaList
                    .First(map => map.Agent.Equals(agent.Id))
                    .ManagementAreas.Select(ma => Areas.First(area => area.Key.Equals(ma)).Value);
                foreach (var area in areas)
                {
                    var key = HarvestResults.GetKey(1, agent, area);
                    _harvested[key] = 0.0;
                }
            }
        }
    }
}
