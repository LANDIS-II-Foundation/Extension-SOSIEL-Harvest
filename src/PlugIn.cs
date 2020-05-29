﻿// This file is part of SHE for LANDIS-II.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Landis.Core;
using Landis.Extension.SOSIELHarvest.Algorithm;
using Landis.Extension.SOSIELHarvest.Configuration;
using Landis.Extension.SOSIELHarvest.Helpers;
using Landis.Extension.SOSIELHarvest.Input;
using Landis.Extension.SOSIELHarvest.Models;
using Landis.Extension.SOSIELHarvest.Services;
using Landis.Library.BiomassCohorts;
using Landis.Library.HarvestManagement;
using Landis.SpatialModeling;
using Landis.Utilities;

namespace Landis.Extension.SOSIELHarvest
{
    public class PlugIn : ExtensionMain
    {
        public static readonly ExtensionType ExtType = new ExtensionType("disturbance:harvest");

        public static readonly string ExtensionName = "SOSIEL Harvest";

        private SheParameters _sheParameters;
        private SosielParameters _sosielParameters;
        private BiomassHarvest.Parameters _biomassHarvestParameters;

        private ConfigurationModel _configuration;

        private AlgorithmModel sosielHarvestModel;
        private SosielHarvestImplementation sosielHarvest;
        private BiomassHarvest.PlugIn _biomassHarvest;
        private IManagementAreaDataset _biomassHarvestAreas;
        private List<ExtendedPrescription> _extendedPrescriptions;


        private static ICore modelCore;


        private Dictionary<Area, double> projectedBiomass;

        private int iteration = 1;

        private LogService _logService;


        //---------------------------------------------------------------------

        public PlugIn()
            : base(ExtensionName, ExtType)
        {
            _logService = new LogService();
            _logService.StartService();
            _extendedPrescriptions = new List<ExtendedPrescription>();
        }

        //---------------------------------------------------------------------

        public static ICore ModelCore => modelCore;
        //---------------------------------------------------------------------

        public override void LoadParameters(string dataFile, ICore mCore)
        {
#if DEBUG
            Debugger.Launch();
#endif
            modelCore = mCore;

            ModelCore.UI.WriteLine("  Loading parameters from {0}", dataFile);
            var sheParameterParser = new SheParameterParser();
            _sheParameters = Data.Load(dataFile, sheParameterParser);

            ModelCore.UI.WriteLine("  Loading parameters from {0}", _sheParameters.SosielInitializationFileName);
            var sosielParameterParser = new SosielParameterParser();
            _sosielParameters = Data.Load(_sheParameters.SosielInitializationFileName, sosielParameterParser);

            if (_sheParameters.Mode == 2)
            {
                ModelCore.UI.WriteLine("  Loading parameters from {0}",
                    _sheParameters.BiomassHarvestInitializationFileName);
                _biomassHarvest = new BiomassHarvest.PlugIn();
                _biomassHarvest.LoadParameters(_sheParameters.BiomassHarvestInitializationFileName, ModelCore);
            }
        }

        //---------------------------------------------------------------------

        public override void Initialize()
        {
#if DEBUG
            Debugger.Launch();
#endif
            ModelCore.UI.WriteLine("Initializing {0}...", Name);
            //SiteVars.Initialize();
            Timestep = _sheParameters.Timestep;
            _configuration = ConfigurationParser.MakeConfiguration(_sosielParameters);
            ////create algorithm instance
            int iterations = 1; // Later we can decide if there should be multiple SHE sub-iterations per LANDIS-II iteration. 
            sosielHarvestModel = new AlgorithmModel();
            var managementAreas = _sheParameters.AgentToManagementAreaList.GroupBy(m => m.ManagementArea)
                .Select(g => new Area() {Name = g.Key, AssignedAgents = g.Select(m => m.Agent).ToArray()})
                .ToArray();
            sosielHarvest =
                new SosielHarvestImplementation(iterations, _configuration, managementAreas);
            sosielHarvest.Initialize(sosielHarvestModel);
            //remove old output files
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(System.IO.Directory.GetCurrentDirectory());

            foreach (System.IO.FileInfo fi in di.GetFiles("SOSIELHuman_*.csv"))
            {
                fi.Delete();
            }

            if (_sheParameters.Mode == 2)
            {
                _biomassHarvest.Initialize();

                var biomassHarvestPluginType = _biomassHarvest.GetType();
                var prescriptionField = biomassHarvestPluginType.GetField("managementAreas",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                if (prescriptionField == null)
                    throw new Exception();
                _biomassHarvestAreas = (IManagementAreaDataset) prescriptionField.GetValue(_biomassHarvest);

                var parametersField =
                    biomassHarvestPluginType.GetField("parameters", BindingFlags.Static | BindingFlags.NonPublic);
                if (parametersField == null)
                    throw new Exception();
                _biomassHarvestParameters = (BiomassHarvest.Parameters) parametersField.GetValue(_biomassHarvest);

                foreach (var biomassHarvestArea in _biomassHarvestAreas)
                {
                    foreach (var appliedPrescription in biomassHarvestArea.Prescriptions)
                    {
                        _extendedPrescriptions.Add(appliedPrescription.ToExtendedPrescription());
                    }
                }
            } 
        }

        public override void Run()
        {
            try
            {
                RunBiomassHarvest();
            }
            catch (Exception e)
            {
                _logService.WriteLine("Exception: " + e.Message);
                _logService.StopService();
                throw;
            }

            if (ModelCore.CurrentTime == ModelCore.EndTime)
                _logService.StopService();
        }

        private void RunBiomassHarvest()
        {
            _logService.WriteLine("Timestamp:\t" + ModelCore.CurrentTime);

            if (_sheParameters.Mode == 2)
            {
                sosielHarvestModel.HarvestResults = AnalyzeHarvestResult();
                _logService.WriteLine("\tRun Sosiel with parameters:");
                foreach (var pair in sosielHarvestModel.HarvestResults.ManageAreaBiomass)
                {
                    _logService.WriteLine(
                        $"\t\t{"Biomass:",-20}{sosielHarvestModel.HarvestResults.ManageAreaBiomass[pair.Key],10:N0}");
                    _logService.WriteLine(
                        $"\t\t{"Harvested:",-20}{sosielHarvestModel.HarvestResults.ManageAreaHarvested[pair.Key],10:N0}");
                    _logService.WriteLine(
                        $"\t\t{"MaturityProportion:",-20}{sosielHarvestModel.HarvestResults.ManageAreaMaturityProportion[pair.Key],10:F2}");
                }

                var model = sosielHarvest.Run(sosielHarvestModel);

                foreach (var decisionOptionModel in model.NewDecisionOptions)
                {
                    GenerateNewPrescription(decisionOptionModel.Name, decisionOptionModel.ConsequentVariable,
                        decisionOptionModel.ConsequentValue, decisionOptionModel.BasedOn,
                        decisionOptionModel.ManagementArea);
                }

                if (model.NewDecisionOptions.Any())
                {
                    _logService.WriteLine("\tSosiel generated new prescriptions:");
                    _logService.WriteLine($"\t\t{"Area",-10}{"Name",-20}{"Based on",-20}{"Variable",-40}{"Value",10}");

                    foreach (var decisionOption in model.NewDecisionOptions)
                    {
                        _logService.WriteLine(
                            $"\t\t{decisionOption.ManagementArea,-10}{decisionOption.Name,-20}{decisionOption.BasedOn,-20}{decisionOption.ConsequentVariable,-40}{decisionOption.ConsequentValue,10}");
                    }
                }

                _logService.WriteLine($"\tSosiel selected next prescriptions:");
                _logService.WriteLine($"\t\t{"Area",-10}Prescriptions");

                foreach (var selectedDecisionPair in model.SelectedDecisions)
                {
                    if (selectedDecisionPair.Value.Count == 0)
                    {
                        _logService.WriteLine($"\t\t{selectedDecisionPair.Key,-10}none");
                        continue;
                    }

                    var managementArea = _biomassHarvestAreas.ToArray()[int.Parse(selectedDecisionPair.Key) - 1];

                    managementArea.Prescriptions.Clear();

                    foreach (var selectedDesignName in selectedDecisionPair.Value)
                    {
                        var extendedPrescription =
                            _extendedPrescriptions.First(ep => ep.Name.Equals(selectedDesignName));
                        ApplyPrescription(managementArea, extendedPrescription);
                    }

                    var prescriptions = selectedDecisionPair.Value.Aggregate((s1, s2) => $"{s1} {s2}");

                    _logService.WriteLine($"\t\t{selectedDecisionPair.Key,-10}{prescriptions}");
                }

                _biomassHarvest.Run();
            }

            iteration++;
        }

        private HarvestResults AnalyzeHarvestResult()
        {
            var results = new HarvestResults();

            int areaNumber = 0;

            foreach (var biomassHarvestArea in _biomassHarvestAreas)
            {
                areaNumber++;

                results.ManageAreaBiomass[areaNumber.ToString()] = 0;
                results.ManageAreaHarvested[areaNumber.ToString()] = 0;
                results.ManageAreaMaturityProportion[areaNumber.ToString()] = 0;

                double manageAreaMaturityProportion = 0;

                foreach (var stand in biomassHarvestArea)
                {
                    double standMaturityProportion = 0;

                    foreach (var site in stand)
                    {
                        double siteBiomass = 0;
                        double siteMaturity = 0;
                        double siteMaturityProportion = 0;

                        foreach (var species in modelCore.Species)
                        {
                            var cohorts = BiomassHarvest.SiteVars.Cohorts[site][species];

                            if (cohorts == null)
                                continue;

                            double siteSpeciesMaturity = 0;

                            foreach (var cohort in cohorts)
                            {
                                siteBiomass += cohort.Biomass;

                                if (cohort.Age >= modelCore.Species[species.Name].Maturity)
                                    siteSpeciesMaturity += cohort.Biomass;
                            }

                            siteMaturity += siteSpeciesMaturity;
                        }

                        siteMaturityProportion = Math.Abs(siteBiomass) < 0.0001 ? 0 : siteMaturity / siteBiomass;
                        standMaturityProportion += siteMaturityProportion;

                        results.ManageAreaBiomass[areaNumber.ToString()] += siteBiomass;
                        results.ManageAreaHarvested[areaNumber.ToString()] +=
                            BiomassHarvest.SiteVars.BiomassRemoved[site];
                    }

                    standMaturityProportion /= stand.Count();
                    manageAreaMaturityProportion += standMaturityProportion;
                }

                manageAreaMaturityProportion /= biomassHarvestArea.StandCount;

                results.ManageAreaBiomass[areaNumber.ToString()] =
                    results.ManageAreaBiomass[areaNumber.ToString()] / 100 * modelCore.CellArea;

                results.ManageAreaHarvested[areaNumber.ToString()] =
                    results.ManageAreaHarvested[areaNumber.ToString()] / 100 * modelCore.CellArea;

                results.ManageAreaMaturityProportion[areaNumber.ToString()] = manageAreaMaturityProportion;
            }

            return results;
        }

        private void GenerateNewPrescription(string newName, string parameter, dynamic value, string basedOn,
            string managementAreaName)
        {
            var originalPrescription = _extendedPrescriptions.First(p => p.Name.Equals(basedOn)).Prescription;

            Prescription newPrescription;

            var managementArea = _biomassHarvestAreas.ToArray()[int.Parse(managementAreaName) - 1];

            AppliedPrescription appliedPrescription = null;

            appliedPrescription =
                managementArea.Prescriptions.First(p => p.Prescription.Name.Equals(basedOn));

            var areaToHarvest = appliedPrescription.PercentageToHarvest;
            var standsToHarvest = appliedPrescription.PercentStandsToHarvest;
            var beginTime = appliedPrescription.BeginTime;
            var endTime = appliedPrescription.EndTime;

            switch (parameter)
            {
                case "HarvestArea":
                    areaToHarvest = new Percentage(value);
                    newPrescription = originalPrescription.Copy(newName, null);
                    break;
                case "CohortsRemovedProportionAdjustment":
                    newPrescription = originalPrescription.Copy(newName, (double) value);
                    break;
                default:
                    throw new Exception();
            }

            _extendedPrescriptions.Add(new ExtendedPrescription(newPrescription, areaToHarvest, standsToHarvest,
                beginTime, endTime));
        }

        private void ApplyPrescription(ManagementArea managementArea, ExtendedPrescription extendedPrescription)
        {
            managementArea.ApplyPrescription(extendedPrescription.Prescription,
                new Percentage(extendedPrescription.HarvestAreaPercent),
                new Percentage(extendedPrescription.HarvestStandsAreaPercent), extendedPrescription.StartTime,
                extendedPrescription.EndTime);
            managementArea.FinishInitialization();
        }

        //---------------------------------------------------------------------

        private static double ComputeSiteBiomass(ActiveSite site)
        {
            double siteBiomass = 0;
            if (SiteVars.Cohorts[site] != null)
                foreach (ISpeciesCohorts speciesCohorts in SiteVars.Cohorts[site])
                foreach (ICohort cohort in speciesCohorts)
                    siteBiomass += cohort.Biomass;
            return siteBiomass;
        }

        private static void WriteSheLog(string logEntry)
        {
            ModelCore.UI.WriteLine(logEntry);
        }
    }
}