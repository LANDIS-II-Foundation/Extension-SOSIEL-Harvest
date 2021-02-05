// Name: PlugIn.cs
// Description: 
// Authors: Multiple.
// Last updated: July 10th, 2020.
// Copyright: Garry Sotnik, Brooke A. Cassell, Robert M. Scheller.

using System;
using System.Diagnostics;
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

        private static ICore _core;

        private readonly LogService _logService;


        //---------------------------------------------------------------------

        public PlugIn()
            : base(ExtensionName, ExtType)
        {
            _logService = new LogService();
            _logService.StartService();
        }

        //---------------------------------------------------------------------

        public static ICore ModelCore => _core;
        //---------------------------------------------------------------------

        public override void LoadParameters(string dataFile, ICore mCore)
        {
#if DEBUG
            Debugger.Launch();
#endif
            _core = mCore;
            Main.InitializeLib(_core);

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

            if (_sheParameters.Mode == 1)
                _mode = new Mode1(_core, _sheParameters);
            else if (_sheParameters.Mode == 2)
                _mode = new Mode2(_biomassHarvest);

            _mode.Initialize();

            //create algorithm instance
            _sosielData = new SosielData(_sheParameters.Mode);
            _sosielHarvestAlgorithm = new SosielHarvestAlgorithm(iterations, _configuration, _mode.Areas.Values);
            _sosielHarvestAlgorithm.Initialize(_sosielData);
            
            _mode.SetAgents(_sosielHarvestAlgorithm.ActiveAgents);

            //remove old output files
            var di = new System.IO.DirectoryInfo(System.IO.Directory.GetCurrentDirectory());
            foreach (System.IO.FileInfo fi in di.GetFiles("output_SOSIEL_Harvest*.csv"))
                fi.Delete();
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
                _logService.WriteLine("Exception: " + e.Message);
                _logService.StopService();
                throw;
            }

            if (ModelCore.CurrentTime == ModelCore.EndTime)
                _logService.StopService();
        }

        private void RunSosiel()
        {
            _logService.WriteLine("\tRun Sosiel with parameters:");
            foreach (var pair in _sosielData.HarvestResults.ManageAreaBiomass)
            {
                _logService.WriteLine(
                    $"\t\t{"Biomass:",-20}{_sosielData.HarvestResults.ManageAreaBiomass[pair.Key],10:N0}");
                _logService.WriteLine(
                    $"\t\t{"Harvested:",-20}{_sosielData.HarvestResults.ManageAreaHarvested[pair.Key],10:N0}");
                _logService.WriteLine(
                    $"\t\t{"MaturityPercent:",-20}{_sosielData.HarvestResults.ManageAreaMaturityPercent[pair.Key],10:F2}");
            }

            _sosielHarvestAlgorithm.Run(_sosielData);
        }
    }
}