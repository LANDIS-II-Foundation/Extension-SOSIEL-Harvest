// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using CsvHelper;

using Landis.Extension.SOSIELHarvest.Configuration;
using Landis.Extension.SOSIELHarvest.Models;
using Landis.Extension.SOSIELHarvest.Services;
using Landis.Utilities;

using SOSIEL.Enums;

using StringReader = Landis.Utilities.StringReader;

namespace Landis.Extension.SOSIELHarvest.Input
{
    public class SosielParameterParser : TextParser<SosielParameters>
    {
        private readonly LogService _log;

        public override string LandisDataValue => PlugIn.ExtensionName;

        public SosielParameterParser(LogService log)
        {
            _log = log;
        }

        protected override SosielParameters Parse()
        {
            return new SosielParameters
            {
                CognitiveLevel = ReadCognitiveLevel(),
                GoalPrioritizingConfiguration = ReadGoalPrioritizingConfiguration(),
                GoalAttributes = ReadGoalAttributes(),
                MentalModels = ReadMentalModels(),
                DecisionOptionAttributes = ReadDecisionOptionAttributes(),
                DecisionOptionAntecedentAttributes = ReadDecisionOptionAntecedentAttributes(),
                AgentArchetypes = ReadAgentArchetypes(),
                AgentArchetypeVariables = ReadAgentArchetypeVariables(),
                AgentGoalAttributes = ReadAgentGoalAttributes(),
                AgentVariables = ReadAgentVariables(),
                AgentDecisionOptions = ReadAgentDecisionOptions(),
                Demographic = ReadDemographic(),
                Probabilities = ReadProbabilities(),
            };
        }

        private CognitiveLevel ReadCognitiveLevel()
        {
            var cognitiveLevel = new InputVar<string>("CognitiveLevel");
            ReadVar(cognitiveLevel);
            return (CognitiveLevel) Enum.Parse(typeof(CognitiveLevel), cognitiveLevel.Value);
        }

        private GoalPrioritizingConfiguration ReadGoalPrioritizingConfiguration()
        {
            if (CurrentName == "GoalPrioritizing")
            {
                var currentLine = new StringReader(CurrentLine);
                GetNextLine();
                var gp = new InputVar<string>("GoalPrioritizing");
                ReadValue(gp, currentLine);
                var goalPrioritizingType = new InputVar<string>("GoalPrioritizingType");
                ReadValue(goalPrioritizingType, currentLine);

                var type = (GoalPrioritizingType)Enum.Parse(typeof(GoalPrioritizingType), goalPrioritizingType.Value);
                switch (type)
                {
                    case GoalPrioritizingType.Default: break;
                    case GoalPrioritizingType.VBGP:
                    {
                        var configFile = new InputVar<string>("GoalPrioritizingConfig");
                        ReadValue(configFile, currentLine);
                        return new GoalPrioritizingConfiguration
                        {
                            GoalPrioritizingType = GoalPrioritizingType.VBGP,
                            ConfigFile = configFile.Value.String
                        };
                    }
                }
            }
            return new GoalPrioritizingConfiguration
            {
                GoalPrioritizingType = GoalPrioritizingType.Default,
                ConfigFile = ""
            };
        }

        private Demographic ReadDemographic()
        {
            var demographicAttributes = new InputVar<string>("DemographicAttributes");
            ReadVar(demographicAttributes);
            var fileName = demographicAttributes.Value;
            using (var reader = new StreamReader(fileName))
            {
                using (var csv = new CsvReader(reader, CSVReaderConfig.config))
                {
                    var records = csv.GetRecords<Demographic>().ToList();
                    var demographic = new Demographic();
                    demographic = records.FirstOrDefault();
                    return demographic;
                }
            }
        }

        private List<Probability> ReadProbabilities()
        {
            SkipUntilConfiguration("ProbabilityAttributes");
            var variableParameter = new InputVar<string>("VariableParameter");
            var variableType = new InputVar<string>("VariableType");
            var fileName = new InputVar<string>("FileName");
            var ignoreFirstLine = new InputVar<bool>("IgnoreFirstLine");

            var probabilities = new List<Probability>();
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

        private List<GoalAttribute> ReadGoalAttributes()
        {
            SkipUntilConfiguration("GoalAttributes");
            var agentArchetype = new InputVar<string>("AgentArchetype");
            var goalName = new InputVar<string>("Goal");
            var goalTendency = new InputVar<string>("GoalTendency");
            var referenceVariable = new InputVar<string>("ReferenceVariable");
            var changeValueOnPrior = new InputVar<bool>("ChangeValueOnPrior");
            var isCumulative = new InputVar<bool>("IsCumulative");
            var focalValueReferenceVariable = new InputVar<string>("FocalValueReferenceVariable");

            var goals = new List<GoalAttribute>();
            while (CurrentName != "MentalModelAttributes")
            {
                var currentLine = new StringReader(CurrentLine);
                var goal = new GoalAttribute();

                ReadValue(agentArchetype, currentLine);
                goal.AgentArchetype = agentArchetype.Value;

                ReadValue(goalName, currentLine);
                goal.Name = goalName.Value;

                ReadValue(goalTendency, currentLine);
                goal.Tendency = (GoalTendency) Enum.Parse(typeof(GoalTendency), goalTendency.Value);

                ReadValue(referenceVariable, currentLine);
                goal.ReferenceVariable = referenceVariable.Value;

                ReadValue(changeValueOnPrior, currentLine);
                goal.ChangeValueOnPrior = changeValueOnPrior.Value;

                ReadValue(isCumulative, currentLine);
                goal.IsCumulative = isCumulative.Value;

                ReadValue(focalValueReferenceVariable, currentLine);
                goal.FocalValueReferenceVariable =
                    focalValueReferenceVariable.Value == "-" ? "" : focalValueReferenceVariable.Value;

                goals.Add(goal);
                GetNextLine();
            }
            return goals;
        }

        private List<MentalModel> ReadMentalModels()
        {
            SkipUntilConfiguration("MentalModelAttributes");
            var agentArchetype = new InputVar<string>("AgentArchetype");
            var name = new InputVar<string>("Name");
            var modifiable = new InputVar<bool>("Modifiable");
            var maxNumberOfDecisionOptions = new InputVar<int>("MaxNumberOfDecisionOptions");
            var decisionOptionGoalRelationship = new InputVar<string>("DecisionOptionGoalRelationship");
            var associatedWithGoals = new InputVar<string>("AssociatedWithGoals");
            var consequentValueRange = new InputVar<string>("ConsequentValueRange");
            var consequentRound = new InputVar<int>("ConsequentRound");

            // Debugger.Launch();
            var mentalModels = new List<MentalModel>();
            while (CurrentName != "DecisionOptionAttributes")
            {
                var stringReader = new StringReader(CurrentLine);
                var mentalModel = new MentalModel();

                ReadValue(agentArchetype, stringReader);
                mentalModel.AgentArchetype = agentArchetype.Value;

                ReadValue(name, stringReader);
                mentalModel.Name = name.Value;

                ReadValue(modifiable, stringReader);
                mentalModel.Modifiable = modifiable.Value;

                ReadValue(maxNumberOfDecisionOptions, stringReader);
                mentalModel.MaxNumberOfDecisionOptions = maxNumberOfDecisionOptions.Value;

                ReadValue(decisionOptionGoalRelationship, stringReader);
                mentalModel.DecisionOptionGoalRelationship = decisionOptionGoalRelationship.Value;

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

        private List<DecisionOptionAttribute> ReadDecisionOptionAttributes()
        {
            var doAttributesFileName = new InputVar<string>("DecisionOptionAttributes");
            ReadVar(doAttributesFileName);
            var fileName = doAttributesFileName.Value;
            _log.WriteLine($"  Loading DecisionOptionAttributes from {fileName}");

            using (var reader = new StreamReader(fileName))
            {
                using (var csv = new CsvReader(reader, CSVReaderConfig.config))
                {
                    var records = csv.GetRecords<DecisionOptionAttribute>();
                    var decisionOptionAttributes = new List<DecisionOptionAttribute>();
                    decisionOptionAttributes.AddRange(records);
                    return decisionOptionAttributes;
                }
            }
        }

        private List<DecisionOptionAntecedentAttribute> ReadDecisionOptionAntecedentAttributes()
        {
            var doAntecedentAttributesFileName = new InputVar<string>("DecisionOptionAntecedentAttributes");
            ReadVar(doAntecedentAttributesFileName);
            var fileName = doAntecedentAttributesFileName.Value;
            _log.WriteLine($"  Loading DecisionOptionAntecedentAttributes from {fileName}");
            using (var reader = new StreamReader(fileName))
            {
                using (var csv = new CsvReader(reader, CSVReaderConfig.config))
                {
                    var records = csv.GetRecords<DecisionOptionAntecedentAttribute>();
                    var decisionOptionAntecedentAttributes = new List<DecisionOptionAntecedentAttribute>();
                    decisionOptionAntecedentAttributes.AddRange(records);
                    return decisionOptionAntecedentAttributes;
                }
            }
        }

        private List<AgentArchetype> ReadAgentArchetypes()
        {
            SkipUntilConfiguration("AgentArchetypeAttributes");
            var archetypeName = new InputVar<string>("ArchetypeName");
            var archetypePrefix = new InputVar<string>("ArchetypePrefix");
            var dataSetOriented = new InputVar<bool>("DataSetOriented");
            var goalImportanceAdjusting = new InputVar<bool>("GoalImportanceAdjusting");

            var agentArchetypes = new List<AgentArchetype>();
            while (!CurrentName.Equals("AgentArchetypeVariables"))
            {
                var currentLine = new StringReader(CurrentLine);
                var agentArchetype = new AgentArchetype();

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

        private List<AgentArchetypeVariable> ReadAgentArchetypeVariables()
        {
            SkipUntilConfiguration("AgentArchetypeVariables");
            var variableName = new InputVar<string>("VariableName");
            var archetypeName = new InputVar<string>("ArchetypeName");
            var variableType = new InputVar<string>("VariableType");
            var variableValue = new InputVar<string>("VariableValue");

            var agentArchetypeVariables = new List<AgentArchetypeVariable>();
            while (!CurrentName.Equals("AgentGoalAttributes"))
            {
                var currentLine = new StringReader(CurrentLine);
                var agentArchetypeVariable = new AgentArchetypeVariable();

                // 2021-01-21 Archetype name must come first
                ReadValue(archetypeName, currentLine);
                agentArchetypeVariable.ArchetypeName = archetypeName.Value;

                ReadValue(variableName, currentLine);
                agentArchetypeVariable.VariableName = variableName.Value;

                ReadValue(variableType, currentLine);
                agentArchetypeVariable.VariableType = variableType.Value;

                ReadValue(variableValue, currentLine);
                agentArchetypeVariable.VariableValue = variableValue.Value;

                agentArchetypeVariables.Add(agentArchetypeVariable);
                GetNextLine();
            }
            return agentArchetypeVariables;
        }

        private List<AgentGoalAttribute> ReadAgentGoalAttributes()
        {
            var agentGoalAttributesFileName = new InputVar<string>("AgentGoalAttributes");
            ReadVar(agentGoalAttributesFileName);
            var fileName = agentGoalAttributesFileName.Value;
            _log.WriteLine($"  Loading AgentGoalAttributes from {fileName}");
            using (var reader = new StreamReader(fileName))
            {
                using (var csv = new CsvReader(reader, CSVReaderConfig.config))
                {
                    var records = csv.GetRecords<AgentGoalAttribute>();
                    var agentGoalAttributes = new List<AgentGoalAttribute>();
                    agentGoalAttributes.AddRange(records);
                    return agentGoalAttributes;
                }
            }
        }

        private List<AgentVariable> ReadAgentVariables()
        {
            var agentVariablesFileName = new InputVar<string>("AgentVariables");
            ReadVar(agentVariablesFileName);
            var fileName = agentVariablesFileName.Value;
            _log.WriteLine($"  Loading AgentVariables from {fileName}");
            using (var reader = new StreamReader(fileName))
            {
                using (var csv = new CsvReader(reader, CSVReaderConfig.config))
                {
                    var records = csv.GetRecords<AgentVariable>();
                    var agentVariables = new List<AgentVariable>();
                    agentVariables.AddRange(records);
                    return agentVariables;
                }
            }
        }

        private List<AgentDecisionOptions> ReadAgentDecisionOptions()
        {
            var agentDOAttributes = new InputVar<string>("AgentDecisionOptionAttributes");
            ReadVar(agentDOAttributes);
            var fileName = agentDOAttributes.Value;
            _log.WriteLine($"  Loading AgentDecisionOptionAttributes from {fileName}");
            using (var reader = new StreamReader(fileName))
            {
                using (var csv = new CsvReader(reader, CSVReaderConfig.config))
                {
                    var records = csv.GetRecords<AgentDecisionOptions>();
                    var agentDecisionOptions = new List<AgentDecisionOptions>();
                    agentDecisionOptions.AddRange(records);
                    return agentDecisionOptions;
                }
            }
        }

        private void SkipUntilConfiguration(string directive)
        {
            while (CurrentName != directive)
                GetNextLine();
            GetNextLine();
        }
    }
}
