// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Landis.Extension.SOSIELHarvest.Models;
using Landis.Utilities;

namespace Landis.Extension.SOSIELHarvest.Input
{
    /// <summary>
    /// A parser that reads the extension's input and output parameters from
    /// a text file.
    /// </summary>
    public class SheParameterParser : TextParser<SheParameters>
    {
        public override string LandisDataValue => PlugIn.ExtensionName;

        protected override SheParameters Parse()
        {
            ReadLandisDataVar();

            var sheParameters = new SheParameters();

            // Note: Default mode 2 is already set in the SheParameters constructor
            // Debugger.Launch();
            if (CurrentName.Equals("Mode"))
                sheParameters.Modes = ParseModes();

            const string kTimeStep = "Timestep";
            var continueLoop = true;
            while (continueLoop && !string.IsNullOrEmpty(CurrentName))
            {
                // Debugger.Launch();

                switch (CurrentName)
                {
                    case "Mode1SpeciesBiomassLogFile":
                    case "Mode2SpeciesBiomassLogFile":
                    case "Mode3SpeciesBiomassLogFile":
                    {
                        var savedCurrentName = CurrentName;
                        var modeSpeciesBiomassLogFile = new InputVar<string>(CurrentName);
                        ReadVar(modeSpeciesBiomassLogFile);
                        var mode = int.Parse(savedCurrentName.Substring(4, 1));
                        sheParameters.ModeSpecificBiomassLogFiles[mode] = modeSpeciesBiomassLogFile.Value;
                        break;
                    }

                    case "GenerateSpeciesBiomassForManagementAreas":
                    {
                        var generateSpeciesBiomassForManagementAreas = new InputVar<string>(CurrentName);
                        ReadVar(generateSpeciesBiomassForManagementAreas);
                        if (generateSpeciesBiomassForManagementAreas.Value == "all")
                            sheParameters.GenerateSpeciesBiomassForAllManagementAreas = true;
                        else if (generateSpeciesBiomassForManagementAreas.Value == "none")
                        {
                            // do not do anything here
                        }
                        else
                        {
                            foreach (
                                var managementAreaMapCode in
                                generateSpeciesBiomassForManagementAreas.Value.String.Split(',')
                                    .Select(s => uint.Parse(s))
                            )
                            {
                                sheParameters.ManagementAreasToGenerateSpeciesBiomassFor.Add(managementAreaMapCode);
                            }
                        }
                        break;
                    }

                    default:
                    {
                        continueLoop = false;
                        break;
                    }
                }
            }

            if (!CurrentName.Equals(kTimeStep))
                throw new Exception($"Invalid parameter: expecting ${kTimeStep}, but received {CurrentName}");

            var timestep = new InputVar<int>(kTimeStep);
            ReadVar(timestep);
            sheParameters.Timestep = timestep.Value;

            var moduleName = new InputVar<string>("ModuleName");
            var initializationFileName = new InputVar<string>("InitializationFileName");

            while (!CurrentName.Equals("DecisionOptions") && !CurrentName.Equals("AgentToManagementArea"))
            {
                var currentLine = new StringReader(CurrentLine);

                ReadValue(moduleName, currentLine);
                ReadValue(initializationFileName, currentLine);

                switch (moduleName.Value)
                {
                    case "SOSIEL Harvest":
                        sheParameters.SosielInitializationFileName = initializationFileName.Value;
                        break;
                    case "Biomass Harvest":
                        sheParameters.BiomassHarvestInitializationFileName = initializationFileName.Value;
                        break;
                    case "ManagementAreas":
                        sheParameters.ManagementAreaFileName = initializationFileName.Value;
                        break;
                    case "Stands":
                        sheParameters.StandsFileName = initializationFileName.Value;
                        break;
                }

                GetNextLine();
            }

            if (CurrentName.Equals("DecisionOptions"))
                sheParameters.Prescriptions = ParsePrescriptions();

            if (CurrentName.Equals("AgentToManagementArea"))
                sheParameters.AgentToManagementAreaList = ParseAgentToManagementAreaList(sheParameters.Modes);

            return sheParameters;
        }

        private List<int> ParseModes()
        {
            var mode = new InputVar<string>("Mode");
            ReadVar(mode);
            return mode.Value.Actual.Split(',').Select(s => int.Parse(s)).ToList();
        }

        private List<Prescription> ParsePrescriptions()
        {
            var prescriptions = new List<Prescription>();

            while (CurrentName != "DecisionOptions")
                GetNextLine();

            while (CurrentName != "DO")
                GetNextLine();

            while (CurrentName == "DO")
            {
                var moduleName = new InputVar<string>("DO");
                ReadVar(moduleName);

                var targetHarvestSize = new InputVar<float>("TargetHarvestSize");
                ReadVar(targetHarvestSize);

                var prescription = new Prescription(moduleName.Value) {TargetHarvestSize = targetHarvestSize.Value};

                while (CurrentName != "ForestType")
                    GetNextLine();

                GetNextLine();

                while (CurrentName != "CohortsRemoved")
                {
                    var currentLine = new StringReader(CurrentLine);
                    var speciesName = new InputVar<string>("SpeciesName");
                    var speciesAgeString = new InputVar<string>("SpeciesAge");
                    var speciesPercentString = new InputVar<float>("SpeciesPercent");
                    ReadValue(speciesName, currentLine);
                    ReadValue(speciesAgeString, currentLine);
                    ReadValue(speciesPercentString, currentLine);
                    var age = speciesAgeString.Value.String.Split('-');
                    var percent = new Percentage(speciesPercentString.Value / 100);
                    var selectionRule = new SiteSelectionRule(speciesName.Value, int.Parse(age[0]), int.Parse(age[1]),
                        percent);
                    prescription.AddSiteSelectionRule(selectionRule);
                    GetNextLine();
                }

                GetNextLine();

                while (CurrentName != "AgentToManagementArea" && CurrentName != "DO")
                {
                    var currentLine = new StringReader(CurrentLine);
                    var speciesName = new InputVar<string>("SpeciesName");
                    var speciesAgeString = new InputVar<string>("SpeciesAge");
                    var speciesPercentString = new InputVar<float>("SpeciesPercent");
                    ReadValue(speciesName, currentLine);
                    ReadValue(speciesAgeString, currentLine);
                    ReadValue(speciesPercentString, currentLine);
                    var age = speciesAgeString.Value.String.Split('-');
                    var percent = new Percentage(speciesPercentString.Value / 100);
                    var harvestingRule = new SiteHarvestingRule(speciesName.Value, int.Parse(age[0]), int.Parse(age[1]),
                        percent);
                    prescription.AddSiteHarvestingRule(harvestingRule);
                    GetNextLine();
                }

                prescriptions.Add(prescription);
            }

            return prescriptions;
        }

        private List<AgentToManagementArea> ParseAgentToManagementAreaList(IReadOnlyList<int> modes)
        {
            while (CurrentName != "AgentToManagementArea")
                GetNextLine();

            GetNextLine();

            var agentToManagementList = new List<AgentToManagementArea>();
            var agent = new InputVar<string>("Agent");
            var managementArea = new InputVar<string>("ManagementArea");
            var siteSelectionMethod = new InputVar<string>("SiteSelectionMethod");
            var agentMode = new InputVar<int>("AgentMode");
            // var hasMode1 = modes.Contains(1);

            while (!AtEndOfInput && CurrentName != "OUTPUTS")
            {
                var currentLine = new StringReader(CurrentLine);
                ReadValue(agent, currentLine);
                ReadValue(managementArea, currentLine);
                ReadValue(siteSelectionMethod, currentLine);
                ReadValue(agentMode, currentLine);
                var agentToManagementArea = new AgentToManagementArea()
                {
                    Agent = agent.Value,
                    SiteSelectionMethod =
                        (SiteSelectionMethod)Enum.Parse(typeof(SiteSelectionMethod), siteSelectionMethod.Value.String),
                    AgentMode = agentMode.Value
                };
                agentToManagementArea.ManagementAreas.AddRange(managementArea.Value.String.Split(','));
                agentToManagementList.Add(agentToManagementArea);
                GetNextLine();
            }

            return agentToManagementList;
        }
    }
}
