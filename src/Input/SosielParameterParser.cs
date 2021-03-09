/// Name: SosielParameterParser.cs
/// Description:
/// Authors: Multiple.
/// Copyright: Garry Sotnik, Brooke A. Cassell, Robert M. Scheller.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using Landis.Extension.SOSIELHarvest.Models;
using Landis.Extension.SOSIELHarvest.Services;
using Landis.Utilities;
using SOSIEL.Enums;
using StringReader = Landis.Utilities.StringReader;

namespace Landis.Extension.SOSIELHarvest.Input
{
    public class SosielParameterParser : TextParser<SosielParameters>
    {
        public override string LandisDataValue => PlugIn.ExtensionName;

        private readonly LogService _logService;

        private CsvHelper.Configuration.Configuration _csvReaderConfig =
            new CsvHelper.Configuration.Configuration(CultureInfo.InvariantCulture);

        public SosielParameterParser(LogService logService)
        {
            _logService = logService;
        }

        protected override SosielParameters Parse()
        {
            var sosielParameters = new SosielParameters
            {
                CognitiveLevel = ParseCognitiveLevel(),
                GoalAttributes = ParseGoalAttributes(),
                MentalModels = ParseMentalModels(),
                DecisionOptionAttributes = ParseDecisionOptionAttributes(),
                DecisionOptionAntecedentAttributes = ParseDecisionOptionAntecedentAttributes(),
                AgentArchetypes = ParseAgentArchetypes(),
                AgentArchetypeVariables = ParseAgentArchetypeVariables(),
                AgentGoalAttributes = ParseAgentGoalAttributes(),
                AgentVariables = ParseAgentVariables(),
                AgentDecisionOptions = ParseAgentDecisionOptions(),
                Demographic = ParseDemographic(),
                Probabilities = ParseProbabilities(),
            };

            return sosielParameters;
        }

        private CognitiveLevel ParseCognitiveLevel()
        {
            InputVar<string> cognitiveLevel =
                new InputVar<string>("CognitiveLevel");
            ReadVar(cognitiveLevel);
            return (CognitiveLevel)Enum.Parse(typeof(CognitiveLevel), cognitiveLevel.Value);
        }

        private Demographic ParseDemographic()
        {


            var demographic = new Demographic();

            InputVar<string> demographicAttributes =
                new InputVar<string>("DemographicAttributes");
            ReadVar(demographicAttributes);
            var fileName = demographicAttributes.Value;

            using (var reader = new StreamReader(fileName))
            {
                using (var csv = new CsvReader(reader, _csvReaderConfig))
                {
                    var records = csv.GetRecords<Demographic>().ToList();
                    demographic = records.FirstOrDefault();
                }
            }
            return demographic;
        }

        private List<Probability> ParseProbabilities()
        {
            var probabilities = new List<Probability>();

            while (CurrentName != "ProbabilityAttributes")
                GetNextLine();

            GetNextLine();

            var variableParameter = new InputVar<string>("VariableParameter");
            var variableType = new InputVar<string>("VariableType");
            var fileName = new InputVar<string>("FileName");
            var ignoreFirstLine = new InputVar<bool>("IgnoreFirstLine");

            while (!string.IsNullOrEmpty(CurrentLine))
            {
                var probability = new Probability();

                var currentLine = new StringReader(CurrentLine);

                ReadValue(variableParameter, currentLine);
                probability.VariableParameter = variableParameter.Value;

                ReadValue(variableType, currentLine);
                probability.VariableType = variableType.Value;

                ReadValue(fileName, currentLine);
                probability.FileName = fileName.Value;

                ReadValue(ignoreFirstLine, currentLine);
                probability.IgnoreFirstLine = ignoreFirstLine.Value;

                probabilities.Add(probability);

                GetNextLine();
            }

            return probabilities;
        }

        private List<GoalAttribute> ParseGoalAttributes()
        {
            var goals = new List<GoalAttribute>();

            while (CurrentName != "GoalAttributes")
                GetNextLine();

            GetNextLine();

            var agentArchetype = new InputVar<string>("AgentArchetype");
            var goalName = new InputVar<string>("Goal");
            var goalTendency = new InputVar<string>("GoalTendency");
            var referenceVariable = new InputVar<string>("ReferenceVariable");
            var changeValueOnPrior = new InputVar<bool>("ChangeValueOnPrior");
            var isCumulative = new InputVar<bool>("IsCumulative");

            while (CurrentName != "MentalModelAttributes")
            {
                var goal = new GoalAttribute();

                var currentLine = new StringReader(CurrentLine);

                ReadValue(agentArchetype, currentLine);
                goal.AgentArchetype = agentArchetype.Value;

                ReadValue(goalName, currentLine);
                goal.Name = goalName.Value;

                ReadValue(goalTendency, currentLine);
                goal.GoalTendency = goalTendency.Value;

                ReadValue(referenceVariable, currentLine);
                goal.ReferenceVariable = referenceVariable.Value;

                ReadValue(changeValueOnPrior, currentLine);
                goal.ChangeValueOnPrior = changeValueOnPrior.Value;

                ReadValue(isCumulative, currentLine);
                goal.IsCumulative = isCumulative.Value;

                goals.Add(goal);

                GetNextLine();
            }

            return goals;
        }

        private List<MentalModel> ParseMentalModels()
        {
            var mentalModels = new List<MentalModel>();

            while (CurrentName != "MentalModelAttributes")
                GetNextLine();

            GetNextLine();

            var agentArchetype = new InputVar<string>("AgentArchetype");
            var name = new InputVar<string>("Name");
            var modifiable = new InputVar<bool>("Modifiable");
            var maxNumberOfDesignOptions = new InputVar<int>("MaxNumberOfDesignOptions");
            var designOptionGoalRelationship = new InputVar<string>("DesignOptionGoalRelationship");
            var associatedWithGoals = new InputVar<string>("AssociatedWithGoals");
            var consequentValueRange = new InputVar<string>("ConsequentValueRange");
            var consequentRound = new InputVar<string>("ConsequentRound");

            while (CurrentName != "DecisionOptionAttributes")
            {
                var mentalModel = new MentalModel();

                var stringReader = new StringReader(CurrentLine);

                ReadValue(agentArchetype, stringReader);
                mentalModel.AgentArchetype = agentArchetype.Value;

                ReadValue(name, stringReader);
                mentalModel.Name = name.Value;

                ReadValue(modifiable, stringReader);
                mentalModel.Modifiable = modifiable.Value;

                ReadValue(maxNumberOfDesignOptions, stringReader);
                mentalModel.MaxNumberOfDesignOptions = maxNumberOfDesignOptions.Value;

                ReadValue(designOptionGoalRelationship, stringReader);
                mentalModel.DesignOptionGoalRelationship = designOptionGoalRelationship.Value;

                ReadValue(associatedWithGoals, stringReader);
                mentalModel.AssociatedWithGoals = associatedWithGoals.Value;

                if (stringReader.Index < CurrentLine.Length)
                {
                    ReadValue(consequentValueRange, stringReader);
                    mentalModel.ConsequentValueRange = consequentValueRange.Value;

                    ReadValue(consequentRound, stringReader);
                    mentalModel.ConsequentRound = consequentRound.Value;
                }

                mentalModels.Add(mentalModel);

                GetNextLine();
            }

            return mentalModels;
        }

        private List<DecisionOptionAttribute> ParseDecisionOptionAttributes()
        {
            var decisionOptionAttributes = new List<DecisionOptionAttribute>();

            var decisionOptionAttributesFileName = new InputVar<string>("DecisionOptionAttributes");
            ReadVar(decisionOptionAttributesFileName);
            var fileName = decisionOptionAttributesFileName.Value;
            _logService.WriteLine($"  Loading DecisionOptionAttributes from {fileName}");

            using (var reader = new StreamReader(fileName))
            {
                using (var csv = new CsvReader(reader, _csvReaderConfig))
                {
                    var list = csv.GetRecords<DecisionOptionAttribute>();
                    decisionOptionAttributes.AddRange(list);
                }
            }

            return decisionOptionAttributes;
        }

        private List<DecisionOptionAntecedentAttribute> ParseDecisionOptionAntecedentAttributes()
        {
            var вDecisionOptionAntecedentAttributes = new List<DecisionOptionAntecedentAttribute>();

            var decisionOptionAntecedentAttributesFileName =
                new InputVar<string>("DecisionOptionAntecedentAttributes");
            ReadVar(decisionOptionAntecedentAttributesFileName);
            var fileName = decisionOptionAntecedentAttributesFileName.Value;
            _logService.WriteLine($"  Loading DecisionOptionAntecedentAttributes from {fileName}");

            using (var reader = new StreamReader(fileName))
            {
                using (var csv = new CsvReader(reader, _csvReaderConfig))
                {
                    var list = csv.GetRecords<DecisionOptionAntecedentAttribute>();
                    вDecisionOptionAntecedentAttributes.AddRange(list);
                }
            }

            return вDecisionOptionAntecedentAttributes;
        }

        private List<AgentArchetype> ParseAgentArchetypes()
        {
            var agentArchetypes = new List<AgentArchetype>();

            while (CurrentName != "AgentArchetypeAttributes")
                GetNextLine();

            GetNextLine();

            var archetypeName = new InputVar<string>("ArchetypeName");
            var archetypePrefix = new InputVar<string>("ArchetypePrefix");
            var dataSetOriented = new InputVar<bool>("DataSetOriented");
            var goalImportanceAdjusting = new InputVar<bool>("GoalImportanceAdjusting");

            while (!CurrentName.Equals("AgentArchetypeVariables"))
            {
                var agentArchetype = new AgentArchetype();

                var currentLine = new StringReader(CurrentLine);

                ReadValue(archetypeName, currentLine);
                agentArchetype.ArchetypeName = archetypeName.Value;

                ReadValue(archetypePrefix, currentLine);
                agentArchetype.ArchetypePrefix = archetypePrefix.Value;

                ReadValue(dataSetOriented, currentLine);
                agentArchetype.DataSetOriented = dataSetOriented.Value;

                ReadValue(goalImportanceAdjusting, currentLine);
                agentArchetype.GoalImportanceAdjusting = goalImportanceAdjusting.Value;

                agentArchetypes.Add(agentArchetype);

                GetNextLine();
            }

            return agentArchetypes;
        }

        private List<AgentArchetypeVariable> ParseAgentArchetypeVariables()
        {
            var agentArchetypeVariables = new List<AgentArchetypeVariable>();

            while (CurrentName != "AgentArchetypeVariables")
                GetNextLine();

            GetNextLine();

            var variableName = new InputVar<string>("VariableName");
            var archetypeName = new InputVar<string>("ArchetypeName");
            var variableType = new InputVar<string>("VariableType");
            var variableValue = new InputVar<string>("VariableValue");

            while (!CurrentName.Equals("AgentGoalAttributes"))
            {
                var agentArchetypeVariable = new AgentArchetypeVariable();

                var currentLine = new StringReader(CurrentLine);

                ReadValue(variableName, currentLine);
                agentArchetypeVariable.VariableName = variableName.Value;

                ReadValue(archetypeName, currentLine);
                agentArchetypeVariable.ArchetypeName = archetypeName.Value;

                ReadValue(variableType, currentLine);
                agentArchetypeVariable.VariableType = variableType.Value;

                ReadValue(variableValue, currentLine);
                agentArchetypeVariable.VariableValue = variableValue.Value;

                agentArchetypeVariables.Add(agentArchetypeVariable);

                GetNextLine();
            }

            return agentArchetypeVariables;
        }

        private List<AgentGoalAttribute> ParseAgentGoalAttributes()
        {
            var agentGoalAttributes = new List<AgentGoalAttribute>();

            var agentGoalAttributesFileName = new InputVar<string>("AgentGoalAttributes");
            ReadVar(agentGoalAttributesFileName);
            var fileName = agentGoalAttributesFileName.Value;
            _logService.WriteLine($"  Loading AgentGoalAttributes from {fileName}");

            using (var reader = new StreamReader(fileName))
            {
                using (var csv = new CsvReader(reader, _csvReaderConfig))
                {
                    var list = csv.GetRecords<AgentGoalAttribute>();
                    agentGoalAttributes.AddRange(list);
                }
            }

            return agentGoalAttributes;
        }

        private List<AgentVariable> ParseAgentVariables()
        {
            var agentVariables = new List<AgentVariable>();

            InputVar<string> agentVariablesFileName =
                new InputVar<string>("AgentVariables");
            ReadVar(agentVariablesFileName);
            var fileName = agentVariablesFileName.Value;
            _logService.WriteLine($"  Loading AgentVariables from {fileName}");

            using (var reader = new StreamReader(fileName))
            {
                using (var csv = new CsvReader(reader, _csvReaderConfig))
                {
                    var list = csv.GetRecords<AgentVariable>();
                    agentVariables.AddRange(list);
                }
            }

            return agentVariables;
        }

        private List<AgentDecisionOption> ParseAgentDecisionOptions()
        {
            var agentDecisionOptions = new List<AgentDecisionOption>();

            InputVar<string> agentDecisionOptionAttributes =
                new InputVar<string>("AgentDecisionOptionAttributes");
            ReadVar(agentDecisionOptionAttributes);
            var fileName = agentDecisionOptionAttributes.Value;
            _logService.WriteLine($"  Loading AgentDecisionOptionAttributes from {fileName}");

            using (var reader = new StreamReader(fileName))
            {
                using (var csv = new CsvReader(reader, _csvReaderConfig))
                {
                    var list = csv.GetRecords<AgentDecisionOption>();
                    agentDecisionOptions.AddRange(list);
                }
            }

            return agentDecisionOptions;
        }
    }
}
