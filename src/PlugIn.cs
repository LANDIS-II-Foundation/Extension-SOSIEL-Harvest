// This file is part of the SOSIEL Harvest Extension (SHE) for LANDIS-II.

// Can use classes from the listed namespaces.
using Landis.Core;
using Landis.Library.BiomassCohorts;
using Landis.SpatialModeling;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Landis.Extension.SOSIELHuman.Algorithm;

namespace Landis.Extension.SOSIELHuman
{
    using Configuration;
    //using Algorithm;
    using System;

    public class PlugIn
        : Landis.Core.ExtensionMain
    {

        public static readonly ExtensionType ExtType = new ExtensionType("disturbance:harvest");
        public static readonly string ExtensionName = "SOSIEL Human";

        private Parameters parameters;


        private ConfigurationModel configuration;

        private AlgorithmModel luhyModel;
        private LuhyLiteImplementation luhyLite;


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
#if DEBUG
            Debugger.Launch();
#endif

            modelCore = mCore;

            ModelCore.UI.WriteLine("  Loading parameters from {0}", dataFile);

            //Parse Landis parameters here
            ParameterParser parser = new ParameterParser(); // ModelCore.Species);
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

            luhyModel = new AlgorithmModel();
            luhyLite = new LuhyLiteImplementation(iterations, configuration, ModelCore.Landscape, projectedBiomass);

            luhyLite.Initialize(luhyModel);


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
            luhyLite.Run(luhyModel);

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
/*
Copyright 2020 Garry Sotnik

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/
