// Copyright (C) 2021 SOSIEL Inc. All rights reserved.
// Use of this source code is governed by a license that can be found
// in the LICENSE file located in the repository root directory.

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

            if (CurrentName.Equals("AgentToMode"))
                sheParameters.AgentToMode = ParseAgentToMode();
            else
            {
                if (sheParameters.Modes.Count > 1)
                    throw new Exception("Missing mapping of agents to modes");
            }

            var timestep = new InputVar<int>("Timestep");
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

        private Dictionary<string, int> ParseAgentToMode()
        {
            while (CurrentName != "AgentToMode")
                GetNextLine();

            GetNextLine();

            var agentToMode = new Dictionary<string, int>();
            var agent = new InputVar<string>("Agent");
            var mode = new InputVar<int>("Mode");
            while (!AtEndOfInput && CurrentName != "Timestep")
            {
                var currentLine = new StringReader(CurrentLine);
                ReadValue(agent, currentLine);
                ReadValue(mode, currentLine);
                agentToMode[agent.Value] = mode.Value;
                GetNextLine();
            }

            return agentToMode;
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
            var hasMode1 = modes.Contains(1);

            while (!AtEndOfInput && CurrentName != "OUTPUTS")
            {
                var agentToManagementArea = new AgentToManagementArea();

                var currentLine = new StringReader(CurrentLine);

                ReadValue(agent, currentLine);
                agentToManagementArea.Agent = agent.Value;

                ReadValue(managementArea, currentLine);
                agentToManagementArea.ManagementAreas.AddRange(managementArea.Value.String.Split(','));

                if (hasMode1)
                {
                    ReadValue(siteSelectionMethod, currentLine);
                    agentToManagementArea.SiteSelectionMethod =
                        (SiteSelectionMethod) Enum.Parse(typeof(SiteSelectionMethod), siteSelectionMethod.Value.String);
                }

                agentToManagementList.Add(agentToManagementArea);

                GetNextLine();
            }

            return agentToManagementList;
        }
    }
}
