/// Name: SosielParameterParser.cs
/// Description:
/// Authors: Multiple.
/// Copyright: Garry Sotnik, Brooke A. Cassell, Robert M. Scheller.

// We have to use method System.AppDomain.AppendPrivatePath(string) declared obsolete
// since that what's we have in the .NET Standard 2.0, so just suppress the warning
#pragma warning disable 618

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

using CsvHelper;

using Landis.Extension.SOSIELHarvest.Models;
using Landis.Extension.SOSIELHarvest.Services;
using Landis.Extension.SOSIELHarvest.SOSIELExtensions.Attributes;
using Landis.Extension.SOSIELHarvest.SOSIELExtensions.Interfaces;
using Landis.Utilities;

using Newtonsoft.Json;

using SOSIEL.Enums;
using SOSIEL.Processes;

using StringReader = Landis.Utilities.StringReader;

namespace Landis.Extension.SOSIELHarvest.Input
{
    public class SOSIELConigurationException : ApplicationException
    {
        public SOSIELConigurationException(string message)
            : base(message)
        {
        }
    }

    public class SosielParameterParser : TextParser<SosielParameters>
    {
        private class SOSIELExtensionCustomProperty
        {
            [JsonProperty("key")]
            public string Key { get; set; }

            [JsonProperty("value")]
            public string Value { get; set; }

            [JsonProperty("comments")]
            public string[] Comments { get; set; }
        };

        private class SOSIELExtensionInfo
        {
            [JsonProperty("category")]
            public string Category { get; set; }

            [JsonProperty("id")] 
            public string Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("description")]
            public string[] Description { get; set; }

            [JsonProperty("assemblyPath")]
            public string AssemblyPath { get; set; }

            [JsonProperty("configurable")]
            public bool Configurable { get; set; }

            [JsonProperty("configurationDirective")]
            public string ConfigurationDirective { get; set; }

            [JsonProperty("customProperties")]
            public SOSIELExtensionCustomProperty[] CustomProperties { get; set; }

            [JsonIgnore]
            public string ManifestPath { get; set; }

            [JsonIgnore]
            public ISOSIELExtension Extension { get; set; }
        }

        private static readonly string kLowercaseManifestSuffix = ".sosielext.json";

        private readonly LogService _logger;

        private CultureInfo _csvCulture = CultureInfo.InvariantCulture;

        private readonly Dictionary<string, Dictionary<string, SOSIELExtensionInfo>> _sosielExtensions =
            new Dictionary<string, Dictionary<string, SOSIELExtensionInfo>>();

        public override string LandisDataValue => PlugIn.ExtensionName;

        public SosielParameterParser(LogService log)
        {
            _logger = log;
        }

        protected override SosielParameters Parse()
        {
            _logger.WriteLine($"  *** Welcome to the {PlugIn.ExtensionName}" +
                $" {Assembly.GetExecutingAssembly().GetName().Version} Extension ***");

            var extensionFolder = ParseExtensionFolder();
            if (!string.IsNullOrEmpty(extensionFolder))
                FindSOSIELExtensions(extensionFolder);

            var sosielParameters = new SosielParameters
            {
                CognitiveLevel = ParseCognitiveLevel(),
                GoalPrioritizing = ParseGoalPrioritizing(),
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
                Probabilities = ParseProbabilities()
            };

            return sosielParameters;
        }

        private string ParseExtensionFolder()
        {
            if (CurrentName != "ExtensionFolder") return "";
            var extensionFolder = new InputVar<string>("ExtensionFolder");
            ReadVar(extensionFolder);
            var s = extensionFolder.Value.String.Trim();
            
            // Remove quotes
            if (s[0] == '"' || s[0] == '\'') s = s.Substring(1);
            if (s.Length > 0 && (s[s.Length - 1] == '"' || s[s.Length - 1] == '\''))
                s = s.Substring(0, s.Length - 1);
            s = s.Trim();

            // Remove trailing directory separator
            if (s.Length > 0 && (s[s.Length - 1] == Path.DirectorySeparatorChar
                || s[s.Length - 1] == Path.AltDirectorySeparatorChar))
            {
                s = s.Substring(0, s.Length - 1);
            }
            
            return s;
        }

        private CognitiveLevel ParseCognitiveLevel()
        {
            var cognitiveLevel = new InputVar<string>("CognitiveLevel");
            ReadVar(cognitiveLevel);
            return (CognitiveLevel)Enum.Parse(typeof(CognitiveLevel), cognitiveLevel.Value);
        }

        private Demographic ParseDemographic()
        {
            var demographic = new Demographic();

            var demographicAttributes = new InputVar<string>("DemographicAttributes");
            ReadVar(demographicAttributes);
            var fileName = demographicAttributes.Value;

            using (var reader = new StreamReader(fileName))
            using (var csv = new CsvReader(reader, _csvCulture))
            {
                var records = csv.GetRecords<Demographic>().ToList();
                demographic = records.FirstOrDefault();
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
            var goalType = new InputVar<string>("GoalType");
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

                ReadValue(goalType, currentLine);
                goal.GoalType = (GoalType)Enum.Parse(typeof(GoalType), goalType.Value);

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

        private IGoalPrioritizing ParseGoalPrioritizing()
        {
            Dictionary<string, SOSIELExtensionInfo> gpExtensions;
            if (!_sosielExtensions.TryGetValue(SOSIELExtensionPropertyValue.kGoalPrioritizingExtensionCategory,
                    out gpExtensions)) return null;

            var extensionInfo = gpExtensions.Where(kvp => CurrentName == kvp.Value.ConfigurationDirective)
                    .Select(kvp => kvp.Value).FirstOrDefault();
            if (extensionInfo == null) return null;

            _logger.WriteLine($"  Processing custom goal prioritizing configuration '{CurrentName}'");

            var line = CurrentLine;
            GetNextLine();

            var config = "";
            if (extensionInfo.Configurable)
            {
                var configValue = new InputVar<string>(extensionInfo.ConfigurationDirective);
                var currentLine = new StringReader(line);
                ReadValue(configValue, currentLine);
                ReadValue(configValue, currentLine);
                config = configValue.Value;
            }

            var assembly = Assembly.LoadFrom(extensionInfo.AssemblyPath);
            var extensionFactoryType = typeof(ISOSIELExtensionFactory);
            var factoryType = assembly.GetExportedTypes().Where(
                    t => t.IsClass 
                    && extensionFactoryType.IsAssignableFrom(t)
                    && t.GetCustomAttributes(typeof(SOSIELExtensionFactoryAttribute), false).Where(
                            attr => ((SOSIELExtensionFactoryAttribute)attr).ExtensionId == extensionInfo.Id).Any())
                .FirstOrDefault();

            if (factoryType == null)
            {
                throw new SOSIELConigurationException($"Extension factory for the SOSIEL extension "
                    + $"'{extensionInfo.Id}' not found in the assembly '{extensionInfo.AssemblyPath}'");
            }

            _logger.WriteLine($"  Creating instance of the factory class {factoryType.FullName} " +
                $"for the SOSIEL extension '{extensionInfo.Id}' ...");
            var constructorParameters = new object[1];
            constructorParameters[0] = _logger;
            using (var extensionFactory = (ISOSIELExtensionFactory)Activator.CreateInstance(
                factoryType, constructorParameters))
            {
                _logger.WriteLine($"  Preparing parameters for the SOSIEL extension '{extensionInfo.Id}' ...");
                var parameters = new Dictionary<string, string>();
                parameters.Add(SOSIELExtensionParameterName.kExtensionId, extensionInfo.Id);
                parameters.Add(SOSIELExtensionParameterName.kConfig, config);
                if (extensionInfo.CustomProperties != null)
                {
                    foreach (var customProperty in extensionInfo.CustomProperties)
                    {
                        if (customProperty != null && !parameters.ContainsKey(customProperty.Key))
                            parameters.Add(customProperty.Key, customProperty.Value);
                    }
                }
                _logger.WriteLine($"  Creating instance of the SOSIEL extension '{extensionInfo.Id}' ...");
                extensionInfo.Extension = extensionFactory.CreateInstance(parameters);
            }

            return (IGoalPrioritizing)extensionInfo.Extension.ExtensionObject;
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

            using (var reader = new StreamReader(fileName))
            using (var csv = new CsvReader(reader, _csvCulture))
            {
                var list = csv.GetRecords<DecisionOptionAttribute>();
                decisionOptionAttributes.AddRange(list);
            }

            return decisionOptionAttributes;
        }

        private List<DecisionOptionAntecedentAttribute> ParseDecisionOptionAntecedentAttributes()
        {
            var decisionOptionAntecedentAttributes = new List<DecisionOptionAntecedentAttribute>();

            var decisionOptionAntecedentAttributesFileName =
                new InputVar<string>("DecisionOptionAntecedentAttributes");
            ReadVar(decisionOptionAntecedentAttributesFileName);
            var fileName = decisionOptionAntecedentAttributesFileName.Value;

            using (var reader = new StreamReader(fileName))
            using (var csv = new CsvReader(reader, _csvCulture))
            {
                var list = csv.GetRecords<DecisionOptionAntecedentAttribute>();
                decisionOptionAntecedentAttributes.AddRange(list);
            }

            return decisionOptionAntecedentAttributes;
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

            using (var reader = new StreamReader(fileName))
            using (var csv = new CsvReader(reader, _csvCulture))
            {
                var list = csv.GetRecords<AgentGoalAttribute>();
                agentGoalAttributes.AddRange(list);
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

            using (var reader = new StreamReader(fileName))
            using (var csv = new CsvReader(reader, _csvCulture))
            {
                var list = csv.GetRecords<AgentVariable>();
                agentVariables.AddRange(list);
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

            using (var reader = new StreamReader(fileName))
            using (var csv = new CsvReader(reader, _csvCulture))
            {
                var list = csv.GetRecords<AgentDecisionOption>();
                agentDecisionOptions.AddRange(list);
            }

            return agentDecisionOptions;
        }

        private void FindSOSIELExtensions(string path)
        {
            var normalizedPath = NormalizePath(path);
            _logger.WriteLine($"  Searching for SOSIEL extensions in the directory '{normalizedPath}' ...");
            int extensionCount = 0;
            foreach (var manifestPath in SelectExtensionManifests(normalizedPath))
            {
                _logger.WriteLine($"  Inspecting manifest file '{manifestPath}'");
                var extensionInfo = JsonConvert.DeserializeObject<SOSIELExtensionInfo>(File.ReadAllText(manifestPath));
                extensionInfo.ManifestPath = manifestPath;

                Dictionary<string, SOSIELExtensionInfo> extensions;
                if (!_sosielExtensions.TryGetValue(extensionInfo.Category, out extensions))
                {
                    extensions = new Dictionary<string, SOSIELExtensionInfo>();
                    _sosielExtensions.Add(extensionInfo.Category, extensions);
                }

                SOSIELExtensionInfo existingExtensionInfo;
                if (extensions.TryGetValue(extensionInfo.Id, out existingExtensionInfo))
                {
                    throw new SOSIELConigurationException($"Duplicate extension '{extensionInfo.Id}' " +
                        $"in the manifest file {manifestPath}, declared earlier " +
                        $"in the manifest file {extensionInfo.ManifestPath}");
                }

                if (extensionInfo.AssemblyPath.StartsWith(".\\") || extensionInfo.AssemblyPath.StartsWith("./"))
                {
                    extensionInfo.AssemblyPath = Path.Combine(Path.GetDirectoryName(manifestPath),
                        extensionInfo.AssemblyPath.Substring(2));
                }

                _logger.WriteLine($"  Adding extension '{extensionInfo.Id}' of category '{extensionInfo.Category}'");
                extensions.Add(extensionInfo.Id, extensionInfo);
                ++extensionCount;
            }
            _logger.WriteLine($"  Summary: Found {extensionCount} SOSIEL extension(s).");
        }

        private static string NormalizePath(string path)
        {
            return Path.GetFullPath(new Uri(path).LocalPath)
                       .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        private static IEnumerable<string> SelectExtensionManifests(string folder)
        {
            return System.IO.Directory.GetFiles(folder).Where(
                file => file.Length > kLowercaseManifestSuffix.Length
                && file.Substring(file.Length - kLowercaseManifestSuffix.Length).ToLower() == kLowercaseManifestSuffix);
        }

        private static bool IsAvailableDotNetAssembly(string path)
        {
            try
            {
                AssemblyName.GetAssemblyName(path);
                return true;
            }
            catch (BadImageFormatException)
            {
                // not a .NET assembly
                return false;
            }
            catch (Exception)
            {
                // any other issue
                return false;
            }
        }
    }
}
