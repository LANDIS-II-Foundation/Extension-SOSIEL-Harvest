/// Name: PlugIn.cs
/// Description: 
/// Authors: Multiple.
/// Copyright: Garry Sotnik, Brooke A. Cassell, Robert M. Scheller.

using System;
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
        //private string _outputFolder;

        public static ICore Core { get; private set; }

        private LogService _log;


        //---------------------------------------------------------------------

        public PlugIn()
            : base(ExtensionName, ExtType)
        {
            _log = new LogService();
            _log.StartService();
        }
        
        //---------------------------------------------------------------------

        public override void LoadParameters(string dataFile, ICore mCore)
        {


            try
            {

            Core.UI.WriteLine("  Loading parameters from {0}", dataFile);
                var sheParameterParser = new SheParameterParser(ExtensionName);
                _sheParameters = Data.Load(dataFile, sheParameterParser);

            Core.UI.WriteLine("  Loading parameters from {0}", _sheParameters.SosielInitializationFileName);
                var sosielParameterParser = new SosielParameterParser(_log);
                _sosielParameters = Data.Load(_sheParameters.SosielInitializationFileName, sosielParameterParser);

            if (_sheParameters.Mode == 1)
            {
                Main.InitializeLib(Core);
            }
            
                if (_sheParameters.Mode == 2)
                {
                Core.UI.WriteLine("  Loading parameters from {0}",
                        _sheParameters.BiomassHarvestInitializationFileName);
                    _biomassHarvest = new BiomassHarvest.PlugIn();
                _biomassHarvest.LoadParameters(_sheParameters.BiomassHarvestInitializationFileName, Core);
                }
            }
            catch (Exception ex)
            {
                var msg = $"\nException: {ex.GetType().FullName}: {ex.Message}\nStack trace:\n{ex.StackTrace}";
                if (ex.InnerException != null)
                {
                    var sb = new System.Text.StringBuilder();
                    sb.Append(msg);
                    for (Exception innerEx = ex.InnerException; innerEx != null; innerEx = innerEx.InnerException)
                    {
                        sb.Append($"\n... caused by: {innerEx.GetType().FullName}: {innerEx.Message}\nStack trace:\n{innerEx.StackTrace}");
                    }
                    sb.Append("\n");
                    msg = sb.ToString();
                }
                _log.WriteLine(msg);
                _log.StopService();
                throw;
            }
        }

        //---------------------------------------------------------------------

        public override void Initialize()
        {
            Core.UI.WriteLine("Initializing {0}...", Name);

            Timestep = _sheParameters.Timestep;
            _configuration = ConfigurationParser.MakeConfiguration(_sosielParameters);
            // Later we can decide if there should be multiple SHE sub-iterations per LANDIS-II iteration.
            int iterations = 1;


            if (_sheParameters.Mode == 1)
                _mode = new Mode1(Core, _sheParameters);
            else if (_sheParameters.Mode == 2)
                _mode = new Mode2(Core, _sheParameters, _biomassHarvest);

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
            catch (Exception ex)
            {
                var msg = $"\nException: {ex.GetType().FullName}: {ex.Message}\nStack trace:\n{ex.StackTrace}";
                if (ex.InnerException != null)
                {
                    var sb = new System.Text.StringBuilder();
                    sb.Append(msg);
                    for (var innerEx = ex.InnerException; innerEx != null; innerEx = innerEx.InnerException)
                    {
                        sb.Append($"\n... caused by: {innerEx.GetType().FullName}: {innerEx.Message}\nStack trace:\n{innerEx.StackTrace}");
                    }
                    sb.Append("\n");
                    msg = sb.ToString();
                }
                _log.WriteLine(msg);
                _log.StopService();
                throw;
            }

            if (Core.CurrentTime == Core.EndTime)
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