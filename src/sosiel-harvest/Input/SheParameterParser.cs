/// Name: SheParameterParser.cs
/// Description:
/// Authors: Multiple.
/// Copyright: Garry Sotnik, Brooke A. Cassell, Robert M. Scheller.

using System;
using System.Collections.Generic;

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
        readonly string _extensionName;

        public override string LandisDataValue => _extensionName;

        public SheParameterParser(string extensionName)
        {
            _extensionName = extensionName;
        }

        protected override SheParameters Parse()
        {
            ReadLandisDataVar();

            var sheParameters = new SheParameters();

            InputVar<int> mode = new InputVar<int>("Mode");
            ReadVar(mode);
            sheParameters.Mode = mode.Value;

            InputVar<int> timestep = new InputVar<int>("Timestep");
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
                sheParameters.AgentToManagementAreaList = ParseAgentToManagementAreaList(sheParameters.Mode);

            return sheParameters;
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

        private List<AgentToManagementArea> ParseAgentToManagementAreaList(int mode)
        {
            var agentToManagementList = new List<AgentToManagementArea>();

            while (CurrentName != "AgentToManagementArea")
                GetNextLine();

            GetNextLine();

            var agent = new InputVar<string>("Agent");
            var managementArea = new InputVar<string>("ManagementArea");
            var siteSelectionMethod = new InputVar<string>("SiteSelectionMethod");

            while (!AtEndOfInput && CurrentName != "OUTPUTS")
            {
                var agentToManagementArea = new AgentToManagementArea();

                var currentLine = new StringReader(CurrentLine);

                ReadValue(agent, currentLine);
                agentToManagementArea.Agent = agent.Value;

                ReadValue(managementArea, currentLine);
                agentToManagementArea.ManagementAreas.AddRange(managementArea.Value.String.Split(','));

                if (mode == 1)
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