/// Name: SheParameterParser.cs
/// Description:
/// Authors: Multiple.
/// Last updated: July 10th, 2020.
/// Copyright: Garry Sotnik, Brooke A. Cassell, Robert M. Scheller.

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
        public override string LandisDataValue => PlugIn.ExtensionName;

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

            InputVar<string> moduleName = new InputVar<string>("ModuleName");
            InputVar<string> initializationFileName = new InputVar<string>("InitializationFileName");

            var currentLine = new StringReader(CurrentLine);
            ReadValue(moduleName, currentLine);
            ReadValue(initializationFileName, currentLine);
            sheParameters.SosielInitializationFileName = initializationFileName.Value;

            GetNextLine();

            currentLine = new StringReader(CurrentLine);
            ReadValue(moduleName, currentLine);
            ReadValue(initializationFileName, currentLine);
            sheParameters.BiomassHarvestInitializationFileName = initializationFileName.Value;

            sheParameters.AgentToManagementAreaList = ParseAgentToManagementAreaList();

            return sheParameters;
        }

        private List<AgentToManagementArea> ParseAgentToManagementAreaList()
        {
            var agentToManagementList = new List<AgentToManagementArea>();

            while (CurrentName != "AgentToManagementArea")
                GetNextLine();

            GetNextLine();

            var agent = new InputVar<string>("Agent");
            var managementArea = new InputVar<string>("ManagementArea");

            while (!AtEndOfInput)
            {
                var agentToManagementArea = new AgentToManagementArea();

                var currentLine = new StringReader(CurrentLine);

                ReadValue(agent, currentLine);
                agentToManagementArea.Agent = agent.Value;

                ReadValue(managementArea, currentLine);
                agentToManagementArea.ManagementArea = managementArea.Value;

                agentToManagementList.Add(agentToManagementArea);

                GetNextLine();
            }

            return agentToManagementList;
        }
    }
}
