/// Name: Mode2.cs
/// Description: 
/// Authors: Multiple.
/// Copyright: Garry Sotnik, Brooke A. Cassell, Robert M. Scheller.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using Landis.Extension.SOSIELHarvest.Algorithm;
using Landis.Extension.SOSIELHarvest.Helpers;
using Landis.Extension.SOSIELHarvest.Input;
using Landis.Library.HarvestManagement;
using Landis.Utilities;

using HarvestManagement = Landis.Library.HarvestManagement;

namespace Landis.Extension.SOSIELHarvest.Models
{
    public class Mode2 : Mode
    {
        private readonly BiomassHarvest.PlugIn _biomassHarvest;
        private readonly List<ExtendedPrescription> _extendedPrescriptions;

        public Mode2(SheParameters sheParameters, BiomassHarvest.PlugIn biomassHarvest)
            : base(sheParameters)
        {
            _biomassHarvest = biomassHarvest;
            _extendedPrescriptions = new List<ExtendedPrescription>();
        }

        public override void Initialize()
        {
            _biomassHarvest.Initialize();

            var biomassHarvestPluginType = _biomassHarvest.GetType();
            var managementAreasField = biomassHarvestPluginType.GetField("managementAreas",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (managementAreasField == null)
                throw new Exception("Can't get management area list from BHE");

            Areas = ((IManagementAreaDataset) managementAreasField.GetValue(_biomassHarvest)).ToDictionary(
                area => area.MapCode.ToString(),
                managementArea =>
                {
                    var area = new Area();
                    area.Initialize(managementArea);
                    return area;
                });

            foreach (var biomassHarvestArea in Areas.Values)
            {
                foreach (var appliedPrescription in biomassHarvestArea.ManagementArea.Prescriptions)
                {
                    _extendedPrescriptions.Add(
                        appliedPrescription.ToExtendedPrescription(biomassHarvestArea.ManagementArea));
                }
            }

            foreach (var agentToManagementArea in sheParameters.AgentToManagementAreaList)
            {
                foreach (var managementAreaName in agentToManagementArea.ManagementAreas)
                {
                    var area = Areas[managementAreaName];
                    area.AssignedAgents.Add(agentToManagementArea.Agent);
                }
            }
        }

        public override void Harvest(SosielData sosielData)
        {
            PlugIn.ModelCore.UI.WriteLine("Run Mode2 harvesting ...");

            foreach (var decisionOptionModel in sosielData.NewDecisionOptions)
            {
                GenerateNewPrescription(decisionOptionModel.Name, decisionOptionModel.ConsequentVariable,
                    decisionOptionModel.ConsequentValue, decisionOptionModel.BasedOn,
                    decisionOptionModel.ManagementArea);
            }

            foreach (var selectedDecisionPair in sosielData.SelectedDecisions)
            {
                var managementArea = Areas[selectedDecisionPair.Key].ManagementArea;
                managementArea.Prescriptions.RemoveAll(prescription =>
                {
                    var decisionPattern = new Regex(@"(MM\d+-\d+_DO\d+)");
                    return decisionPattern.IsMatch(prescription.Prescription.Name);
                });

                foreach (var selectedDesignName in selectedDecisionPair.Value)
                {
                    var extendedPrescription =
                        _extendedPrescriptions.FirstOrDefault(ep =>
                            ep.ManagementArea.MapCode.Equals(managementArea.MapCode) &&
                            ep.Name.Equals(selectedDesignName));
                    if (extendedPrescription != null)
                        ApplyPrescription(managementArea, extendedPrescription);
                }
            }

            _biomassHarvest.Run();
        }

        public override HarvestResults AnalyzeHarvestingResult()
        {
            var results = new HarvestResults();
            foreach (var biomassHarvestArea in Areas.Values.Select(a => a.ManagementArea))
            {
                var key = biomassHarvestArea.MapCode.ToString();
                results.ManageAreaBiomass[key] = 0;
                results.ManageAreaHarvested[key] = 0;
                results.ManageAreaMaturityPercent[key] = 0;

                double manageAreaMaturityProportion = 0;
                foreach (var stand in biomassHarvestArea)
                {
                    double standMaturityProportion = 0;
                    foreach (var site in stand)
                    {
                        double siteBiomass = 0;
                        double siteMaturity = 0;
                        foreach (var species in PlugIn.ModelCore.Species)
                        {
                            var cohorts = BiomassHarvest.SiteVars.Cohorts[site][species];
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
                        results.ManageAreaHarvested[key] += BiomassHarvest.SiteVars.BiomassRemoved[site];
                    }

                    standMaturityProportion /= stand.Count();
                    manageAreaMaturityProportion += standMaturityProportion;
                }

                manageAreaMaturityProportion /= biomassHarvestArea.StandCount;
                results.ManageAreaBiomass[key] = results.ManageAreaBiomass[key] / 100 * PlugIn.ModelCore.CellArea;
                results.ManageAreaHarvested[key] = results.ManageAreaHarvested[key] / 100 * PlugIn.ModelCore.CellArea;
                results.ManageAreaMaturityPercent[key] = 100 * manageAreaMaturityProportion;
            }
            return results;
        }

        private void GenerateNewPrescription(string newName, string parameter, dynamic value, string basedOn,
            string managementAreaName)
        {
            HarvestManagement.Prescription newPrescription;
            var managementArea = Areas[managementAreaName].ManagementArea;
            var appliedPrescription = managementArea.Prescriptions.First(p => p.Prescription.Name.Equals(basedOn));
            var areaToHarvest = appliedPrescription.PercentageToHarvest;
            var standsToHarvest = appliedPrescription.PercentStandsToHarvest;
            var beginTime = appliedPrescription.BeginTime;
            var endTime = appliedPrescription.EndTime;

            switch (parameter)
            {
                case "PercentOfHarvestArea":
                {
                    var newAreaToHarvest = new Percentage(value / 100);
                    double cuttingMultiplier =
                        areaToHarvest.Value > 0 ? newAreaToHarvest.Value / areaToHarvest.Value : 1;
                    areaToHarvest = newAreaToHarvest;
                    newPrescription = appliedPrescription.Prescription.Copy(newName, cuttingMultiplier);
                    break;
                }
                default: throw new Exception($"Invalid parameter {parameter}");
            }

            _extendedPrescriptions.Add(new ExtendedPrescription(
                newPrescription, managementArea, areaToHarvest, standsToHarvest, beginTime, endTime));
        }

        private void ApplyPrescription(ManagementArea managementArea, ExtendedPrescription extendedPrescription)
        {
            managementArea.ApplyPrescription(extendedPrescription.Prescription,
                new Percentage(extendedPrescription.HarvestAreaPercent),
                new Percentage(extendedPrescription.HarvestStandsAreaPercent), extendedPrescription.StartTime,
                    extendedPrescription.EndTime);
            managementArea.FinishInitialization();
        }
    }
}
