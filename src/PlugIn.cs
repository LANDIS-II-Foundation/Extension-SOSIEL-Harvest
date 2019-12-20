// This file is part of SHE for LANDIS-II.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Landis.Core;
using Landis.Extension.SOSIELHarvest.Algorithm;
using Landis.Extension.SOSIELHarvest.Configuration;
using Landis.Extension.SOSIELHarvest.Input;
using Landis.Extension.SOSIELHarvest.Models;
using Landis.Library.BiomassCohorts;
using Landis.Library.HarvestManagement;
using Landis.SpatialModeling;
using Landis.Utilities;
using SOSIEL.Configuration;
using SOSIEL.Entities;
using SOSIEL.Enums;

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

        private AlgorithmModel luhyModel;
        private LuhyLiteImplementation luhyLite;
        private BiomassHarvest.PlugIn _biomassHarvest;
        private IManagementAreaDataset _biomassHarvestAreas;
        

        private static ICore modelCore;


        private Dictionary<ActiveSite, double> projectedBiomass;

        private int iteration = 1;


        //---------------------------------------------------------------------

        public PlugIn()
            : base(ExtensionName, ExtType)
        {
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
                ModelCore.UI.WriteLine("  Loading parameters from {0}", _sheParameters.BiomassHarvestInitializationFileName);
                _biomassHarvest = new BiomassHarvest.PlugIn();
                _biomassHarvest.LoadParameters(_sheParameters.BiomassHarvestInitializationFileName, ModelCore);
            }
        }

        //---------------------------------------------------------------------

        public override void Initialize()
        {
            ModelCore.UI.WriteLine("Initializing {0}...", Name); 
            SiteVars.Initialize();

            Timestep = _sheParameters.Timestep;

            _configuration = ConfigurationParser.MakeConfiguration(_sosielParameters);

            //// Read in (input) Agent Configuration Json File here:
            //ModelCore.UI.WriteLine("  Loading agent parameters from {0}", parameters.InputJson);
            //configuration = ConfigurationParser.ParseConfiguration(parameters.InputJson);

            //configuration = new ConfigurationModel() { AlgorithmConfiguration = new AlgorithmConfiguration() { } }


            ////create algorithm instance
            //int iterations = 1; // Later we can decide if there should be multiple SHE sub-iterations per LANDIS-II iteration. 
            ////create dictionary 
            //projectedBiomass = ModelCore.Landscape.ToDictionary(activeSite => activeSite, activeSite => 0d);

            //luhyModel = new AlgorithmModel();
            //luhyLite = new LuhyLiteImplementation(iterations, configuration, ModelCore.Landscape, projectedBiomass);

            //luhyLite.Initialize(luhyModel);


            ////remove old output files
            //System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(System.IO.Directory.GetCurrentDirectory());

            //foreach (System.IO.FileInfo fi in di.GetFiles("SOSIELHuman_*.csv"))
            //{
            //    fi.Delete();
            //}

            if (_sheParameters.Mode == 2)
            {
                _biomassHarvest.Initialize();

                var biomassHarvestPluginType = _biomassHarvest.GetType();
                var prescriptionField = biomassHarvestPluginType.GetField("managementAreas", BindingFlags.Instance | BindingFlags.NonPublic);
                if (prescriptionField == null)
                    throw new Exception();
                _biomassHarvestAreas = (IManagementAreaDataset)prescriptionField.GetValue(_biomassHarvest);

                var parametersField = biomassHarvestPluginType.GetField("parameters", BindingFlags.Static | BindingFlags.NonPublic);
                if (parametersField == null)
                    throw new Exception();
                _biomassHarvestParameters = (BiomassHarvest.Parameters)parametersField.GetValue(_biomassHarvest);
            }
        }

        public override void Run()
        {
            //run SOSIEL algorithm
            //luhyLite.Run(luhyModel);

            if (_sheParameters.Mode == 2)
                _biomassHarvest.Run();

            iteration++;
        }

        private void ChangePrescription()
        {
            foreach (var biomassHarvestArea in _biomassHarvestAreas)
            {
                biomassHarvestArea.Prescriptions.Clear();
                var prescription = _biomassHarvestParameters.Prescriptions.First(p => p.Name.Equals("MM2_DO2"));
                biomassHarvestArea.ApplyPrescription(prescription, new Percentage(0.2), new Percentage(1), 0, 100);
                biomassHarvestArea.FinishInitialization();
            }
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
       
    }
}
