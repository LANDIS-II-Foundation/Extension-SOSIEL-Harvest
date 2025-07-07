// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Landis.Extension.SOSIELHarvest.Algorithm;
using Landis.Extension.SOSIELHarvest.Input;
using Landis.Extension.SOSIELHarvest.Services;

using SOSIEL.Entities;

namespace Landis.Extension.SOSIELHarvest.Models
{
    public abstract class Mode
    {
        private readonly int _modeId;
        private readonly string _speciesBiomassLogFile;
        protected LogService log;
        protected SosielData sosielData;
        protected SosielHarvestAlgorithm sosiel;
        protected readonly SheParameters sheParameters;
        protected IEnumerable<IAgent> Agents { get; private set; }
        public Dictionary<string, Area> Areas { get; set; }
        public int ModeId { get => _modeId; }

        protected Mode(int modeId, PlugIn plugin)
        {
            _modeId = modeId;
            sheParameters = plugin.SheParameters;
            sheParameters.ModeSpecificBiomassLogFiles.TryGetValue(_modeId, out _speciesBiomassLogFile);
            if (_speciesBiomassLogFile != null)
                PlugIn.ModelCore.UI.WriteLine($"  SHE: Mode {_modeId} species biomass log file: {_speciesBiomassLogFile}");
            log = plugin.Log;
            Areas = new Dictionary<string, Area>();
        }

        public void Initialize(PlugIn plugin)
        {
            var ui = PlugIn.ModelCore.UI;

            ui.WriteLine($"  Initializing operation mode #{_modeId}");
            InitializeMode();

            ui.WriteLine("  Creating SOSIEL algorithm instance");
            sosielData = new SosielData();
            sosiel = new SosielHarvestAlgorithm(
                plugin.Log, _modeId, plugin.NumberOfIterations, plugin.Configuration, Areas.Values,
                plugin.SosielParameters.GoalPrioritizingConfiguration);

            ui.WriteLine("  Initializing SOSIEL algorithm instance");
            sosiel.Initialize(sosielData);
            SetAgents(sosiel.ActiveAgents);
        }

        public void SetAgents(IEnumerable<IAgent> agents)
        {
            Agents = agents.ToList();
            OnAgentsSet();
        }

        public void UpdateSpeciesBiomass()
        {
            var speciesByManagementArea = new Dictionary<uint, SpeciesBiomassRecord>();
            foreach (var managementArea in Areas.Values.Select(a => a.ManagementArea))
            {
                // Filter out management areas
                if (
                    !sheParameters.GenerateSpeciesBiomassForAllManagementAreas
                    && !sheParameters.ManagementAreasToGenerateSpeciesBiomassFor.Contains(managementArea.MapCode)
                )
                {
                    continue;
                }

                var r = new SpeciesBiomassRecord(managementArea.MapCode);
                foreach (var species in PlugIn.ModelCore.Species)
                {
                    double biomass = 0.0;
                    foreach (var stand in managementArea)
                    {
                        r.SiteCount += stand.SiteCount;
                        foreach (var site in stand)
                        {
                            var cohorts = BiomassHarvest.SiteVars.Cohorts[site][species];
                            if (cohorts != null)
                            {
                                foreach (var cohort in cohorts)
                                    biomass += cohort.Data.Biomass;
                            }
                        }
                    }
                    r.TotalAboveGroundBiomass[species.Index] = biomass;
                }
                r.UpdateAverageAboveGroundBiomass();
                speciesByManagementArea.Add(managementArea.MapCode, r);
            }
            sosiel.SetSpeciesBiomass(speciesByManagementArea);

            if (_speciesBiomassLogFile != null)
                WriteSpeciesBiomass(speciesByManagementArea);
        }

        private void WriteSpeciesBiomass(IReadOnlyDictionary<uint, SpeciesBiomassRecord>  speciesByManagementArea)
        {
            // Create new log file in the simulation beginning
            if (PlugIn.ModelCore.CurrentTime == 0)
            {
                log.WriteLine($"SHE: Creating species biomass log file '{_speciesBiomassLogFile}'");
                PlugIn.CreateDirectory(_speciesBiomassLogFile);
                using (var w = new StreamWriter(_speciesBiomassLogFile))
                {
                    var h = new StringBuilder();
                    h.Append("Time,ManagementArea,SiteCount");
                    foreach (var species in PlugIn.ModelCore.Species)
                    {
                        h.Append($",TotalAboveGroundBiomass_{species.Name}");
                        h.Append($",AverageAboveGroundBiomass_{species.Name}");
                    }
                    w.WriteLine(h);
                }
            }

            // Append current data to log
            log.WriteLine($"SHE: Writing species biomass to log file '{_speciesBiomassLogFile}'");
            var records = speciesByManagementArea.Values.ToArray();
            Array.Sort(
                records,
                (r1, r2) => r1.ManagementAreaMapCode == r2.ManagementAreaMapCode
                    ? 0 : (r1.ManagementAreaMapCode < r2.ManagementAreaMapCode ? -1 : 1)
            );
            using (var w = new StreamWriter(_speciesBiomassLogFile, true))
            {
                var line = new StringBuilder();
                foreach (var r in records)
                {
                    line.Clear();
                    line.Append($"{PlugIn.ModelCore.CurrentTime},{r.ManagementAreaMapCode},{r.SiteCount}");
                    foreach (var species in PlugIn.ModelCore.Species)
                    {
                        var index = species.Index;
                        line.Append(',').Append(r.TotalAboveGroundBiomass[index]);
                        line.Append(',').Append(r.AverageAboveGroundBiomass[index]);
                    }
                    w.WriteLine(line);
                }
            }
        }

        public void Run()
        {
            sosielData.HarvestResults = AnalyzeHarvestingResult();
            RunSosiel();
            Harvest();
        }

        protected abstract void InitializeMode();

        protected abstract void Harvest();

        protected abstract HarvestResults AnalyzeHarvestingResult();

        protected virtual void OnAgentsSet()
        {
        }

        private void RunSosiel()
        {
            log.WriteLine("\tRun Sosiel with parameters:");
            foreach (var pair in sosielData.HarvestResults.ManagementAreaBiomass)
            {
                log.WriteLine(
                    $"\tArea: {pair.Key}");
                log.WriteLine(
                    $"\t\t{"Biomass: ",-20}{sosielData.HarvestResults.ManagementAreaBiomass[pair.Key],10:N0}");
                log.WriteLine(
                    $"\t\t{"Harvested: ",-20}{sosielData.HarvestResults.ManagementAreaHarvested[pair.Key],10:N0}");
                log.WriteLine(
                    $"\t\t{"MaturityPercent: ",-20}{sosielData.HarvestResults.ManagementAreaMaturityPercent[pair.Key],10:F2}");
            }

            UpdateSpeciesBiomass();
            sosiel.Run(sosielData);

            if (sosielData.NewDecisionOptions.Any())
            {
                log.WriteLine("\tSosiel generated new prescriptions:");
                log.WriteLine($"\t\t{"Area",-10}{"Name",-20}{"Based on",-20}{"Variable",-40}{"Value",10}");

                foreach (var decisionOption in sosielData.NewDecisionOptions)
                {
                    log.WriteLine(
                        $"\t\t{decisionOption.ManagementArea,-10}{decisionOption.Name,-20}{decisionOption.BasedOn,-20}{decisionOption.ConsequentVariable,-40}{decisionOption.ConsequentValue,10}");
                }

                log.WriteLine($"\tSosiel selected the following prescriptions:");
                log.WriteLine($"\t\t{"Area",-10}Prescriptions");

                foreach (var selectedDecisionPair in sosielData.SelectedDecisions)
                {
                    if (selectedDecisionPair.Value.Count == 0)
                    {
                        log.WriteLine($"\t\t{selectedDecisionPair.Key,-10}none");
                        continue;
                    }
                    var prescriptionsLog = selectedDecisionPair.Value.Aggregate((s1, s2) => $"{s1} {s2}");
                    log.WriteLine($"\t\t{selectedDecisionPair.Key,-10}{prescriptionsLog}");
                }
            }
            else
            {
                log.WriteLine("\tSosiel: There are no new prescriptions.");
            }
        }
    }
}
