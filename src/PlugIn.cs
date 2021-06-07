// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using Landis.Core;
using Landis.Extension.SOSIELHarvest.Configuration;
using Landis.Extension.SOSIELHarvest.Input;
using Landis.Extension.SOSIELHarvest.Models;
using Landis.Extension.SOSIELHarvest.Services;

namespace Landis.Extension.SOSIELHarvest
{
    public class PlugIn : ExtensionMain
    {
        public static readonly ExtensionType ExtType = new ExtensionType("disturbance:harvest");
        public static readonly string ExtensionName = "SOSIEL Harvest";

        private readonly int _numberOfIterations;
        private readonly LogService _log;
        private SheParameters _sheParameters;
        private SosielParameters _sosielParameters;
        private ConfigurationModel _configuration;
        private BiomassHarvest.PlugIn _biomassHarvest;
        private List<Mode> _modes;

        internal static ICore ModelCore;

        internal LogService Log { get => _log; }
        internal ConfigurationModel Configuration { get => _configuration; }
        internal int NumberOfIterations { get => _numberOfIterations; }
        internal SheParameters SheParameters { get => _sheParameters; }
        internal BiomassHarvest.PlugIn BiomassHarvest { get => _biomassHarvest; }

        public PlugIn()
            : base(ExtensionName, ExtType)
        {
            // Later we can decide if there should be multiple SHE sub-iterations per LANDIS-II iteration.
            _numberOfIterations = 1;
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

            var initializedHarvestManagement = false;
            foreach (var modeId in _sheParameters.Modes)
            {
                switch (modeId)
                {
                    case 1:
                    case 3:
                    {
                        if (!initializedHarvestManagement && !_sheParameters.Modes.Contains(2))
                        {
                            Landis.Library.HarvestManagement.Main.InitializeLib(ModelCore);
                            initializedHarvestManagement = true;
                        }
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
                        initializedHarvestManagement = true;
                        break;
                    }

                    default: throw new Exception($"Unknown mode {modeId}");
                }
            }

            ModelCore.UI.WriteLine("  All parameters loaded.");
        }

        public override void Initialize()
        {
            ModelCore.UI.WriteLine("Initializing {0}...", Name);
            Timestep = _sheParameters.Timestep;
            _configuration = ConfigurationParser.MakeConfiguration(_sheParameters, _sosielParameters);

            _modes = new List<Mode>();
            foreach (var modeId in _sheParameters.Modes)
            {
                ModelCore.UI.WriteLine($"  Creating operation mode #{modeId} instance");
                Mode mode = null;
                switch (modeId)
                {
                    case 1:
                    {
                        mode = new Mode1(this);
                        break;
                    }

                    case 2:
                    {
                        mode = new Mode2(this);
                        break;
                    }

                    case 3:
                    {
                        mode = new Mode3(this);
                        break;
                    }

                    default: throw new Exception($"Unknown mode {modeId}");
                }
                mode.Initialize(this);
                _modes.Add(mode);
            }

            SiteVars.Initialize();

            ModelCore.UI.WriteLine("  Removing old output files...");
            var di = new System.IO.DirectoryInfo(System.IO.Directory.GetCurrentDirectory());
            foreach (System.IO.FileInfo fi in di.GetFiles("output_SOSIEL_Harvest*.csv"))
                fi.Delete();

            foreach (var mode in _modes)
            {
                mode.UpdateSpeciesBiomass();
            }

            ModelCore.UI.WriteLine("  Initialization complete.");
        }

        public override void Run()
        {
            _log.WriteLine($"SHE: Running for time={ModelCore.CurrentTime}");
            try
            {
                foreach (var mode in _modes)
                {
                    _log.WriteLine($"SHE: ***** Executing mode #{mode.ModeId} *****");
                    mode.Run();
                    _log.WriteLine($"SHE: ***** Finished executing mode #{mode.ModeId} *****");
                }
            }
            catch (Exception e)
            {
                _log.WriteLine($"Exception: {e.GetType().FullName}: {e.Message}");
                var inner = e;
                while (inner.InnerException != null)
                {
                    inner = inner.InnerException;
                    _log.WriteLine($"Caused by: {inner.GetType().FullName}: {inner.Message}");
                }
                _log.StopService();
                throw;
            }

            if (ModelCore.CurrentTime == ModelCore.EndTime)
                _log.StopService();
        }

        public static void CreateDirectory(string path)
        {
            path = path.Trim(null);
            if (path.Length == 0)
                throw new ArgumentException("path is empty or just whitespace");
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
                Landis.Utilities.Directory.EnsureExists(dir);
        }
    }
}
