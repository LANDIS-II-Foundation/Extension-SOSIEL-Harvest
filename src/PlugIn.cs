// Name: PlugIn.cs
// Description: 
// Authors: Multiple.
// Last updated: July 10th, 2020.
// Copyright: Garry Sotnik, Brooke A. Cassell, Robert M. Scheller.

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
using HarvestManagement = Landis.Library.HarvestManagement;
using Prescription = Landis.Extension.SOSIELHarvest.Models.Prescription;


namespace Landis.Extension.SOSIELHarvest
{
    public class PlugIn : ExtensionMain
    {
        public static readonly ExtensionType ExtType = new ExtensionType("disturbance:harvest");

        public static readonly string ExtensionName = "SOSIEL Harvest";

        private SheParameters _sheParameters;
        private SosielParameters _sosielParameters;

        private ConfigurationModel _configuration;

        private AlgorithmModel _algorithmModel;
        private SosielHarvestImplementation sosielHarvest;
        private BiomassHarvest.PlugIn _biomassHarvest;
        private Dictionary<string, Area> _areas;
        private readonly List<ExtendedPrescription> _extendedPrescriptions;
        private List<Prescription> _prescriptions;
        private ISiteVar<ISiteCohorts> _siteCohorts;
        private ISiteVar<string> _harvestPrescriptionName;


        private static ICore _modelCore;


        private Dictionary<Area, double> projectedBiomass;

        private LogService _logService;


        //---------------------------------------------------------------------

        public PlugIn()
            : base(ExtensionName, ExtType)
        {
            _logService = new LogService();
            _logService.StartService();
            _extendedPrescriptions = new List<ExtendedPrescription>();
            _areas = new Dictionary<string, Area>();
        }

        //---------------------------------------------------------------------

        public static ICore ModelCore => _modelCore;
        //---------------------------------------------------------------------

        public override void LoadParameters(string dataFile, ICore mCore)
        {
#if DEBUG
            Debugger.Launch();
#endif

            _modelCore = mCore;
            Main.InitializeLib(_modelCore);

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
            ModelCore.UI.WriteLine("Initializing {0}...", Name);
            Timestep = _sheParameters.Timestep;
            _configuration = ConfigurationParser.MakeConfiguration(_sosielParameters);
            // Later we can decide if there should be multiple SHE sub-iterations per LANDIS-II iteration.
            int iterations = 1;

            _harvestPrescriptionName = _modelCore.GetSiteVar<string>("Harvest.PrescriptionName");

            if (_sheParameters.Mode == 1)
            {
                _prescriptions = _sheParameters.Prescriptions;

                var maDataSet = new ManagementAreaDataset();

                foreach (var agentToManagementArea in _sheParameters.AgentToManagementAreaList)
                {
                    foreach (var managementAreaName in agentToManagementArea.ManagementAreas)
                    {
                        if (_areas.ContainsKey(managementAreaName) == false)
                        {
                            var managementArea = new ManagementArea(ushort.Parse(managementAreaName));
                            maDataSet.Add(managementArea);
                            var newArea = new Area {Name = managementAreaName};
                            newArea.Initialize(managementArea);
                            _areas.Add(managementAreaName, newArea);
                        }

                        var area = _areas[managementAreaName];
                        area.AssignedAgents.Add(agentToManagementArea.Agent);
                    }
                }

                ManagementAreas.ReadMap(_sheParameters.ManagementAreaFileName, maDataSet);
                Stands.ReadMap(_sheParameters.StandsFileName);
                SiteVars.GetExternalVars();
                foreach (ManagementArea mgmtArea in maDataSet)
                    mgmtArea.FinishInitialization();
            }
            else if (_sheParameters.Mode == 2)
            {
                _biomassHarvest.Initialize();

                var biomassHarvestPluginType = _biomassHarvest.GetType();
                var managementAreasField = biomassHarvestPluginType.GetField("managementAreas",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                if (managementAreasField == null)
                    throw new Exception();

                _areas = ((IManagementAreaDataset) managementAreasField.GetValue(_biomassHarvest)).ToDictionary(
                    area => area.MapCode.ToString(), managementArea =>
                    {
                        var area = new Area();
                        area.Initialize(managementArea);
                        return area;
                    });

                foreach (var biomassHarvestArea in _areas.Values)
                {
                    foreach (var appliedPrescription in biomassHarvestArea.ManagementArea.Prescriptions)
                        _extendedPrescriptions.Add(
                            appliedPrescription.ToExtendedPrescription(biomassHarvestArea.ManagementArea));
                }
            }

            //create algorithm instance
            _algorithmModel = new AlgorithmModel {Mode = _sheParameters.Mode};
            sosielHarvest = new SosielHarvestImplementation(iterations, _configuration, _areas.Values);
            sosielHarvest.Initialize(_algorithmModel);

            //remove old output files
            var di = new System.IO.DirectoryInfo(System.IO.Directory.GetCurrentDirectory());
            foreach (System.IO.FileInfo fi in di.GetFiles("output_SOSIEL_Harvest*.csv"))
                fi.Delete();

            _siteCohorts = _modelCore.GetSiteVar<ISiteCohorts>("Succession.BiomassCohorts");
        }

        public override void Run()
        {
            try
            {
                AnalyzeHarvestResult();
                RunSosiel();

                if (_sheParameters.Mode == 1)
                {
                    RunMode1();
                }
                else if (_sheParameters.Mode == 2)
                {
                    //RunBiomassHarvest();
                }
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

        private void RunMode1()
        {
            foreach (var agent in sosielHarvest.ActiveAgents)
            {
                var agentName = agent.Id;
                var agentToManagementArea =
                    _sheParameters.AgentToManagementAreaList.First(a => a.Agent.Equals(agentName));
                foreach (var areaName in agentToManagementArea.ManagementAreas)
                {
                    var area = _areas.First(a => a.Key.Equals(areaName)).Value;
                    var harvestManager =
                        new HarvestManager(area, _prescriptions, _harvestPrescriptionName, _siteCohorts);
                    var harvested = harvestManager.Harvest();
                }
            }
        }

        // private void RunBiomassHarvest()
        // {
        //     _logService.WriteLine("Timestamp:\t" + ModelCore.CurrentTime);
        //
        //     if (_sheParameters.Mode == 2)
        //     {
        //         sosielHarvestModel.HarvestResults = AnalyzeHarvestResult();
        //         _logService.WriteLine("\tRun Sosiel with parameters:");
        //         foreach (var pair in sosielHarvestModel.HarvestResults.ManageAreaBiomass)
        //         {
        //             _logService.WriteLine(
        //                 $"\t\t{"Biomass:",-20}{sosielHarvestModel.HarvestResults.ManageAreaBiomass[pair.Key],10:N0}");
        //             _logService.WriteLine(
        //                 $"\t\t{"Harvested:",-20}{sosielHarvestModel.HarvestResults.ManageAreaHarvested[pair.Key],10:N0}");
        //             _logService.WriteLine(
        //                 $"\t\t{"MaturityPercent:",-20}{sosielHarvestModel.HarvestResults.ManageAreaMaturityPercent[pair.Key],10:F2}");
        //         }
        //
        //         var model = sosielHarvest.Run(sosielHarvestModel);
        //
        //         foreach (var decisionOptionModel in model.NewDecisionOptions)
        //         {
        //             GenerateNewPrescription(decisionOptionModel.Name, decisionOptionModel.ConsequentVariable,
        //                 decisionOptionModel.ConsequentValue, decisionOptionModel.BasedOn,
        //                 decisionOptionModel.ManagementArea);
        //         }
        //
        //         if (model.NewDecisionOptions.Any())
        //         {
        //             _logService.WriteLine("\tSosiel generated new prescriptions:");
        //             _logService.WriteLine($"\t\t{"Area",-10}{"Name",-20}{"Based on",-20}{"Variable",-40}{"Value",10}");
        //
        //             foreach (var decisionOption in model.NewDecisionOptions)
        //             {
        //                 _logService.WriteLine(
        //                     $"\t\t{decisionOption.ManagementArea,-10}{decisionOption.Name,-20}{decisionOption.BasedOn,-20}{decisionOption.ConsequentVariable,-40}{decisionOption.ConsequentValue,10}");
        //             }
        //         }
        //
        //         _logService.WriteLine($"\tSosiel selected the following prescriptions:");
        //         _logService.WriteLine($"\t\t{"Area",-10}Prescriptions");
        //
        //         foreach (var selectedDecisionPair in model.SelectedDecisions)
        //         {
        //             if (selectedDecisionPair.Value.Count == 0)
        //             {
        //                 _logService.WriteLine($"\t\t{selectedDecisionPair.Key,-10}none");
        //                 continue;
        //             }
        //
        //             var managementArea = _areas[uint.Parse(selectedDecisionPair.Key)];
        //
        //             managementArea.Prescriptions.RemoveAll(prescription =>
        //             {
        //                 var decisionPattern = new Regex(@"(MM\d+-\d+_DO\d+)");
        //                 return decisionPattern.IsMatch(prescription.Prescription.Name);
        //             });
        //
        //             foreach (var selectedDesignName in selectedDecisionPair.Value)
        //             {
        //                 var extendedPrescription =
        //                     _extendedPrescriptions.FirstOrDefault(ep =>
        //                         ep.ManagementArea.MapCode.Equals(managementArea.MapCode) &&
        //                         ep.Name.Equals(selectedDesignName));
        //                 if (extendedPrescription != null)
        //                     ApplyPrescription(managementArea, extendedPrescription);
        //             }
        //
        //             var prescriptionsLog = selectedDecisionPair.Value.Aggregate((s1, s2) => $"{s1} {s2}");
        //
        //             _logService.WriteLine($"\t\t{selectedDecisionPair.Key,-10}{prescriptionsLog}");
        //         }
        //
        //         _biomassHarvest.Run();
        //     }
        // }

        private void AnalyzeHarvestResult()
        {
            var results = new HarvestResults();

            foreach (var biomassHarvestArea in _areas.Values)
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

                        foreach (var species in _modelCore.Species)
                        {
                            var cohorts = _siteCohorts[site][species];

                            if (cohorts == null)
                                continue;

                            double siteSpeciesMaturity = 0;

                            foreach (var cohort in cohorts)
                            {
                                siteBiomass += cohort.Biomass;

                                if (cohort.Age >= _modelCore.Species[species.Name].Maturity)
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
                    results.ManageAreaBiomass[managementAreaName] / 100 * _modelCore.CellArea;

                results.ManageAreaHarvested[managementAreaName] =
                    results.ManageAreaHarvested[managementAreaName] / 100 * _modelCore.CellArea;

                results.ManageAreaMaturityPercent[managementAreaName] = 100 * manageAreaMaturityProportion;
            }

            _algorithmModel.HarvestResults = results;
        }

        private void RunSosiel()
        {
            _logService.WriteLine("\tRun Sosiel with parameters:");
            foreach (var pair in _algorithmModel.HarvestResults.ManageAreaBiomass)
            {
                _logService.WriteLine(
                    $"\t\t{"Biomass:",-20}{_algorithmModel.HarvestResults.ManageAreaBiomass[pair.Key],10:N0}");
                _logService.WriteLine(
                    $"\t\t{"Harvested:",-20}{_algorithmModel.HarvestResults.ManageAreaHarvested[pair.Key],10:N0}");
                _logService.WriteLine(
                    $"\t\t{"MaturityPercent:",-20}{_algorithmModel.HarvestResults.ManageAreaMaturityPercent[pair.Key],10:F2}");
            }

            sosielHarvest.Run(_algorithmModel);
        }

        private void GenerateNewPrescription(string newName, string parameter, dynamic value, string basedOn,
            string managementAreaName)
        {
            HarvestManagement.Prescription newPrescription;

            var area = _areas[managementAreaName];

            var appliedPrescription = area.ManagementArea.Prescriptions.First(p => p.Prescription.Name.Equals(basedOn));

            var areaToHarvest = appliedPrescription.PercentageToHarvest;
            var standsToHarvest = appliedPrescription.PercentStandsToHarvest;
            var beginTime = appliedPrescription.BeginTime;
            var endTime = appliedPrescription.EndTime;

            switch (parameter)
            {
                case "PercentOfHarvestArea":
                    var newAreaToHarvest = new Percentage(value / 100);
                    double cuttingMultiplier =
                        areaToHarvest.Value > 0 ? newAreaToHarvest.Value / areaToHarvest.Value : 1;
                    areaToHarvest = newAreaToHarvest;
                    newPrescription = appliedPrescription.Prescription.Copy(newName, cuttingMultiplier);
                    break;
                default:
                    throw new Exception();
            }

            _extendedPrescriptions.Add(new ExtendedPrescription(newPrescription, area.ManagementArea, areaToHarvest,
                standsToHarvest,
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
    }
}