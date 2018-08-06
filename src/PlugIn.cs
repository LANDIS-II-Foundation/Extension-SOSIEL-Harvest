// This file is part of SHE for LANDIS-II.

using Landis.Core;
using Landis.Library.BiomassCohorts;
using Landis.Library.Succession;
using Landis.SpatialModeling;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Landis.Extension.SOSIELHuman
{
    using Configuration;
    using Algorithm;
    using System;

    public class PlugIn
        : Landis.Core.ExtensionMain
    {

        public static readonly ExtensionType ExtType = new ExtensionType("disturbance:SOSIEL Human");
        public static readonly string ExtensionName = "SOSIEL Human";

        private Parameters parameters;


        private ConfigurationModel configuration;


        private IAlgorithm luhyLite;


        private static ICore modelCore;


        private Dictionary<ActiveSite, double> projectedBiomass;

        private int iteration = 1;


        //---------------------------------------------------------------------

        public PlugIn()
            : base(ExtensionName, ExtType)
        {
        }

        //---------------------------------------------------------------------

        public static ICore ModelCore
        {
            get
            {
                return modelCore;
            }
        }
        //---------------------------------------------------------------------

        public override void LoadParameters(string dataFile,
                                            ICore mCore)
        {
            modelCore = mCore;

            ModelCore.UI.WriteLine("  Loading parameters from {0}", dataFile);

            //Parse Landis parameters here
            ParameterParser parser = new ParameterParser(ModelCore.Species);
            parameters = Landis.Data.Load<Parameters>(dataFile, parser);



        }

        //---------------------------------------------------------------------

        public override void Initialize()
        {
#if DEBUG
            Debugger.Launch();
#endif

            ModelCore.UI.WriteLine("Initializing {0}...", Name);
            SiteVars.Initialize();

            Timestep = parameters.Timestep;

            // Read in (input) Agent Configuration Json File here:
            ModelCore.UI.WriteLine("  Loading agent parameters from {0}", parameters.InputJson);
            configuration = ConfigurationParser.ParseConfiguration(parameters.InputJson);


            //create algorithm instance
            int iterations = 1; // Later we can decide if there should be multiple SHE sub-iterations per LANDIS-II iteration. 
            //create dictionary 
            projectedBiomass = ModelCore.Landscape.ToDictionary(activeSite => activeSite, activeSite => 0d);

            luhyLite = new LuhyLiteImplementation(iterations, configuration, ModelCore.Landscape, projectedBiomass);

            luhyLite.Initialize();


            //remove old output files
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(System.IO.Directory.GetCurrentDirectory());

            foreach (System.IO.FileInfo fi in di.GetFiles("SOSIELHuman_*.csv"))
            {
                fi.Delete();
            }
        }

        public override void Run()
        {
#if DEBUG
            Debugger.Launch();
#endif

            //run SOSIEL algorithm
            luhyLite.RunIteration();

            iteration++;
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
