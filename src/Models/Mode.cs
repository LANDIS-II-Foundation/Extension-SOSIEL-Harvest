// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

using System.Collections.Generic;
using System.Linq;

using Landis.Extension.SOSIELHarvest.Algorithm;
using Landis.Extension.SOSIELHarvest.Input;
using Landis.Extension.SOSIELHarvest.Services;

using SOSIEL.Entities;

namespace Landis.Extension.SOSIELHarvest.Models
{
    public abstract class Mode
    {
        private readonly int _modeId;
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
            log = plugin.Log;
            Areas = new Dictionary<string, Area>();
        }

        public void Initialize(PlugIn plugin)
        {
            var ui = PlugIn.ModelCore.UI;

            ui.WriteLine($"  Initializing work mode #{_modeId}");
            InitializeMode();

            ui.WriteLine("  Creating SOSIEL algorithm instance");
            sosielData = new SosielData();
            sosiel = new SosielHarvestAlgorithm(
                plugin.Log, _modeId, plugin.NumberOfIterations, plugin.Configuration, Areas.Values);

            ui.WriteLine("  Initializing SOSIEL algorithm instance");
            sosiel.Initialize(sosielData);
            SetAgents(sosiel.ActiveAgents);
        }

        public void SetAgents(IEnumerable<IAgent> agents)
        {
            Agents = agents.ToList();
            OnAgentsSet();
        }

        public void SetSpeciesBiomass(IReadOnlyList<SpeciesBiomassRecord> speciesBiomassRecords)
        {
            sosiel.SetSpeciesBiomass(speciesBiomassRecords);
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
            foreach (var pair in sosielData.HarvestResults.ManageAreaBiomass)
            {
                log.WriteLine(
                    $"\tArea:{pair.Key}");
                log.WriteLine(
                    $"\t\t{"Biomass:",-20}{sosielData.HarvestResults.ManageAreaBiomass[pair.Key],10:N0}");
                log.WriteLine(
                    $"\t\t{"Harvested:",-20}{sosielData.HarvestResults.ManageAreaHarvested[pair.Key],10:N0}");
                log.WriteLine(
                    $"\t\t{"MaturityPercent:",-20}{sosielData.HarvestResults.ManageAreaMaturityPercent[pair.Key],10:F2}");
            }

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
        }
    }
}
