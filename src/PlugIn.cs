/// Name: PlugIn.cs
/// Description: 
/// Authors: Multiple.
/// Copyright: Garry Sotnik, Brooke A. Cassell, Robert M. Scheller.

using System;
using System.Diagnostics;
using System.Linq;
using Landis.Core;
using Landis.Extension.SOSIELHarvest.Algorithm;
using Landis.Extension.SOSIELHarvest.Configuration;
using Landis.Extension.SOSIELHarvest.Input;
using Landis.Extension.SOSIELHarvest.Models;
using Landis.Extension.SOSIELHarvest.Services;
using Landis.Library.HarvestManagement;


namespace Landis.Extension.SOSIELHarvest
{
    public class PlugIn : ExtensionMain
    {
        public static readonly ExtensionType ExtType = new ExtensionType("disturbance:harvest");
        public static readonly string ExtensionName = "SOSIEL Harvest";

        private SheParameters _sheParameters;
        private SosielParameters _sosielParameters;
        private ConfigurationModel _configuration;

        private SosielData _sosielData;
        private SosielHarvestAlgorithm _sosielHarvestAlgorithm;
        private BiomassHarvest.PlugIn _biomassHarvest;
        private Mode _mode;
        private readonly LogService _log;

        public static ICore ModelCore { get; private set; }

        public PlugIn()
            : base(ExtensionName, ExtType)
        {
            _log = new LogService();
            _log.StartService();
        }

        public override void LoadParameters(string dataFile, ICore modelCore)
        {
            ModelCore = modelCore;

            ModelCore.UI.WriteLine("  Loading SHE parameters from '{0}'", dataFile);
            var sheParameterParser = new SheParameterParser();
            _sheParameters = Data.Load(dataFile, sheParameterParser);

            if (string.IsNullOrEmpty(_sheParameters.SosielInitializationFileName))
                throw new Exception("Missing SOSIEL parameters configuration file name");
            ModelCore.UI.WriteLine("  Loading SOSIEL SHE parameters from {0}",
                _sheParameters.SosielInitializationFileName);
            var sosielParameterParser = new SosielParameterParser(_log);
            _sosielParameters = Data.Load(_sheParameters.SosielInitializationFileName, sosielParameterParser);

            switch (_sheParameters.Mode)
            {
                case 1:
                {
                    Main.InitializeLib(ModelCore);
                    break;
                }

                case 2:
                {
                    if (string.IsNullOrEmpty(_sheParameters.BiomassHarvestInitializationFileName))
                        throw new Exception("Missing BHE configuration file name");
                    ModelCore.UI.WriteLine("  Loading Biomass Harvest Extension parameters from '{0}'",
                        _sheParameters.BiomassHarvestInitializationFileName);
                    _biomassHarvest = new BiomassHarvest.PlugIn();
                    _biomassHarvest.LoadParameters(_sheParameters.BiomassHarvestInitializationFileName, ModelCore);
                    break;
                }

                case 3:
                {
                    Main.InitializeLib(ModelCore);
                    break;
                }

                default: break;
            }

            ModelCore.UI.WriteLine("  All parameters loaded.");
        }

        public override void Initialize()
        {
            ModelCore.UI.WriteLine("Initializing {0}...", Name);
            Timestep = _sheParameters.Timestep;
            _configuration = ConfigurationParser.MakeConfiguration(_sosielParameters);

            // Later we can decide if there should be multiple SHE sub-iterations per LANDIS-II iteration.
            int numberOfIterations = 1;

            ModelCore.UI.WriteLine($"  Creating operation mode #{_sheParameters.Mode} instance");
            switch (_sheParameters.Mode)
            {
                case 1:
                {
                    _mode = new Mode1(_sheParameters);
                    break;
                }

                case 2:
                {
                    _mode = new Mode2(_sheParameters, _biomassHarvest);
                    break;
                }

                case 3:
                {
                    _mode = new Mode3(_sheParameters);
                    break;
                }

                default: break;
            }

            ModelCore.UI.WriteLine($"  Initializing operation mode #{_sheParameters.Mode} instance");
            _mode.Initialize();

            ModelCore.UI.WriteLine("  Creating SOSIEL algorithm instance");
            _sosielData = new SosielData(_sheParameters.Mode);
            _sosielHarvestAlgorithm = new SosielHarvestAlgorithm(
                _log, numberOfIterations, _configuration, _mode.Areas.Values);

            ModelCore.UI.WriteLine("  Initializing SOSIEL algorithm instance");
            _sosielHarvestAlgorithm.Initialize(_sosielData);

            ModelCore.UI.WriteLine("  Setting agents");
            _mode.SetAgents(_sosielHarvestAlgorithm.ActiveAgents);

            ModelCore.UI.WriteLine("  Removing old output files...");
            var di = new System.IO.DirectoryInfo(System.IO.Directory.GetCurrentDirectory());
            foreach (System.IO.FileInfo fi in di.GetFiles("output_SOSIEL_Harvest*.csv"))
                fi.Delete();

            ModelCore.UI.WriteLine("  Initialization complete.");
        }

        public override void Run()
        {
            try
            {
                _sosielData.HarvestResults = _mode.AnalyzeHarvestingResult();
                RunSosiel();
                _mode.Harvest(_sosielData);
            }
            catch (Exception e)
            {
                _log.WriteLine("Exception: " + e.Message);
                _log.StopService();
                throw;
            }

            if (ModelCore.CurrentTime == ModelCore.EndTime)
                _log.StopService();
        }

        private void RunSosiel()
        {
            _log.WriteLine("\tRun Sosiel with parameters:");
            foreach (var pair in _sosielData.HarvestResults.ManageAreaBiomass)
            {
                _log.WriteLine(
                    $"\tArea:{pair.Key}");
                _log.WriteLine(
                    $"\t\t{"Biomass:",-20}{_sosielData.HarvestResults.ManageAreaBiomass[pair.Key],10:N0}");
                _log.WriteLine(
                    $"\t\t{"Harvested:",-20}{_sosielData.HarvestResults.ManageAreaHarvested[pair.Key],10:N0}");
                _log.WriteLine(
                    $"\t\t{"MaturityPercent:",-20}{_sosielData.HarvestResults.ManageAreaMaturityPercent[pair.Key],10:F2}");
            }

            _sosielHarvestAlgorithm.Run(_sosielData);

            if (_sosielData.NewDecisionOptions.Any())
            {
                _log.WriteLine("\tSosiel generated new prescriptions:");
                _log.WriteLine($"\t\t{"Area",-10}{"Name",-20}{"Based on",-20}{"Variable",-40}{"Value",10}");

                foreach (var decisionOption in _sosielData.NewDecisionOptions)
                {
                    _log.WriteLine(
                        $"\t\t{decisionOption.ManagementArea,-10}{decisionOption.Name,-20}{decisionOption.BasedOn,-20}{decisionOption.ConsequentVariable,-40}{decisionOption.ConsequentValue,10}");
                }

                _log.WriteLine($"\tSosiel selected the following prescriptions:");
                _log.WriteLine($"\t\t{"Area",-10}Prescriptions");

                foreach (var selectedDecisionPair in _sosielData.SelectedDecisions)
                {
                    if (selectedDecisionPair.Value.Count == 0)
                    {
                        _log.WriteLine($"\t\t{selectedDecisionPair.Key,-10}none");
                        continue;
                    }
                    var prescriptionsLog = selectedDecisionPair.Value.Aggregate((s1, s2) => $"{s1} {s2}");
                    _log.WriteLine($"\t\t{selectedDecisionPair.Key,-10}{prescriptionsLog}");
                }
            }
        }
    }
}
