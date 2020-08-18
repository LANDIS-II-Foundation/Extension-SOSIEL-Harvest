/// Name: ConfigurationParser.cs
/// Description:
/// Authors: Multiple.
/// Last updated: July 10th, 2020.
/// Copyright: Garry Sotnik, Brooke A. Cassell, Robert M. Scheller.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using CsvHelper.Configuration;
using Landis.Extension.SOSIELHarvest.Input;
using Landis.Extension.SOSIELHarvest.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using SOSIEL.Configuration;
using SOSIEL.Entities;
using SOSIEL.Enums;
using AgentArchetype = SOSIEL.Entities.AgentArchetype;
using Goal = SOSIEL.Entities.Goal;
using MentalModel = Landis.Extension.SOSIELHarvest.Models.MentalModel;
using Type = System.Type;

namespace Landis.Extension.SOSIELHarvest.Configuration
{
    static class MemberInfoExtensions
    {
        internal static bool IsPropertyWithSetter(this MemberInfo member)
        {
            var property = member as PropertyInfo;

            return property?.GetSetMethod(true) != null;
        }
    }

    public static class ConfigurationParser
    {
        private const string IgnoredValue = "--";

        /// <summary>
        /// Contract resolver for setting properties with private set part.
        /// </summary>
        private class PrivateSetterContractResolver : DefaultContractResolver
        {
            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var jProperty = base.CreateProperty(member, memberSerialization);
                if (jProperty.Writable)
                    return jProperty;

                jProperty.Writable = member.IsPropertyWithSetter();

                return jProperty;
            }
        }


        /// <summary>
        /// Converter for casting integer numbers to int instead of decimal.
        /// </summary>
        private class IntConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return (objectType == typeof(int) || objectType == typeof(object));
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Integer)
                {
                    return Convert.ToInt32((object)reader.Value);
                }

                return reader.Value;
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }
        }


        static JsonSerializer serializer;

        static ConfigurationParser()
        {
            serializer = new JsonSerializer();

            serializer.Converters.Add(new IntConverter());
            serializer.ContractResolver = new PrivateSetterContractResolver();
        }

        /// <summary>
        /// Parses all configuration file.
        /// </summary>
        /// <param name="jsonPath"></param>
        /// <returns></returns>
        public static ConfigurationModel ParseConfiguration(string jsonPath)
        {
            if (File.Exists(jsonPath) == false)
                throw new FileNotFoundException(string.Format("Configuration file doesn't found at {0}", jsonPath));

            string jsonContent = File.ReadAllText(jsonPath);

            JToken json = JToken.Parse(jsonContent);

            return json.ToObject<ConfigurationModel>(serializer);
        }


        public static ConfigurationModel MakeConfiguration(SosielParameters parameters)
        {
            var configuration = new ConfigurationModel
            {
                AlgorithmConfiguration =
                    MakeAlgorithmConfiguration(parameters.CognitiveLevel, parameters.Demographic, parameters.Probabilities),
                AgentConfiguration = MakeAgentConfiguration(parameters),
                InitialState = MakeInitialStateConfiguration(parameters)
            };

            return configuration;
        }

        private static AlgorithmConfiguration MakeAlgorithmConfiguration(CognitiveLevel cognitiveLevel, Demographic demographic, IEnumerable<Probability> probabilities)
        {
            return new AlgorithmConfiguration
            {
                CognitiveLevel = cognitiveLevel,
                DemographicConfiguration = new DemographicProcessesConfiguration
                {
                    AdoptionProbability = demographic.AdoptionProbability,
                    BirthProbability = demographic.BirthProbability,
                    DeathProbability = demographic.DeathProbability,
                    HomosexualTypeRate = demographic.HomosexualTypeRate,
                    MinimumAgeForHouseholdHead = demographic.MinimumAgeForHouseholdHead,
                    PairingAgeMax = demographic.PairingAgeMax,
                    PairingAgeMin = demographic.PairingAgeMin,
                    PairingProbability = demographic.PairingProbability,
                    SexualOrientationRate = demographic.SexualOrientationRate,
                    YearsBetweenBirths = demographic.YearsBetweenBirths,
                    MaximumAge = demographic.MaximumAge
                },
                ProbabilitiesConfiguration = probabilities.Select(p => new ProbabilitiesConfiguration
                {
                    FilePath = p.FileName,
                    Variable = p.VariableParameter,
                    VariableType = p.VariableType,
                    WithHeader = !p.IgnoreFirstLine
                }).ToArray(),
                UseDimographicProcesses = demographic.DemographicChange
            };
        }

        private static Dictionary<string, AgentArchetype> MakeAgentConfiguration(SosielParameters parameters)
        {
            var agentArchetypes = new Dictionary<string, AgentArchetype>();

            var goals = ParseGoals(parameters.GoalAttributes);
            var mentalModels = ParseMentalModels(parameters.MentalModels);
            var decisionOptions = ParseDecisionOptions(parameters.DecisionOptionAttributes, parameters.DecisionOptionAntecedentAttributes);


            foreach (var archetype in parameters.AgentArchetypes)
            {
                var agentArchetype = new AgentArchetype();

                agentArchetype.NamePrefix = archetype.ArchetypePrefix;
                agentArchetype.IsDataSetOriented = archetype.DataSetOriented;
                agentArchetype.UseImportanceAdjusting = archetype.GoalImportanceAdjusting;

                agentArchetype.Goals = goals[archetype.ArchetypeName];
                agentArchetype.MentalModel = mentalModels[archetype.ArchetypeName];

                var supportedMentalModels = agentArchetype.MentalModel.Values.Select(mm => mm.Name).ToList();

                agentArchetype.DecisionOptions = decisionOptions.Keys
                    .Where(d => supportedMentalModels.Contains(GetDecisionOptionMentalModelName(d)))
                    .Select(k => decisionOptions[k])
                    .ToList();


                var variablesCollection = parameters.AgentArchetypeVariables.Cast<IVariable>().ToList();
                foreach (var agentVariable in ParseAgentVariables(variablesCollection, agentArchetype.NamePrefix))
                    agentArchetype.CommonVariables.Add(agentVariable.Key, agentVariable.Value);

                agentArchetypes[archetype.ArchetypeName] = agentArchetype;
            }

            return agentArchetypes;
        }

        private static Dictionary<string, DecisionOption> ParseDecisionOptions(List<DecisionOptionAttribute> decisionOptionAttributes, List<DecisionOptionAntecedentAttribute> decisionOptionAntecedentAttributes)
        {
            var decisionOptions = new Dictionary<string, DecisionOption>();

            foreach (var decisionOption in decisionOptionAttributes)
            {
                var antecedents = decisionOptionAntecedentAttributes.Where(a => a.DecisionOption == decisionOption.DecisionOption).ToList();

                var parsedName = ParseDecisionOptionName(decisionOption.DecisionOption);

                var decision = new DecisionOption();

                decision.MentalModel = parsedName.MentalModel;
                decision.DecisionOptionsLayer = parsedName.MentalSubModel;
                decision.PositionNumber = parsedName.DecisionOptionNumber;

                decision.RequiredParticipants = decisionOption.RequiredParticipants;
                decision.Antecedent = antecedents.Select(a => new DecisionOptionAntecedentPart(a.AntecedentVariable, a.AntecedentOperator, ParseToDynamicValue(a.AntecedentValueType, a.AntecedentValue), a.AntecedentReference)).ToArray();
                var consequentValue = ParseToDynamicValue(decisionOption.ConsequentValueType, decisionOption.ConsequentValue);
                decision.Consequent = new DecisionOptionConsequent(decisionOption.ConsequentVariable, consequentValue, decisionOption.ConsequentValueReference);

                decisionOptions.Add(decisionOption.DecisionOption, decision);
            }

            return decisionOptions;
        }

        private static InitialStateConfiguration MakeInitialStateConfiguration(SosielParameters parameters)
        {
            var sameKeys = parameters.AgentGoalAttributes.Select(ga => ga.Agent).Intersect(parameters.AgentDecisionOptions.Select(d => d.Agent)).ToList();

            if (parameters.AgentGoalAttributes.Count != parameters.AgentDecisionOptions.Count || parameters.AgentGoalAttributes.Count != sameKeys.Count)
                throw new ConfigurationException("AgentGoalAttributes is not suitable for AgentDecisionOptions");

            var initialState = parameters.AgentGoalAttributes
                .Join(parameters.AgentDecisionOptions, ga => ga.Agent, d => d.Agent, (goal, decision) => new { GoalAttribute = goal, DecisionAttribute = decision })
                .ToList();

            var parsedStates = new List<AgentStateConfiguration>();

            foreach (var state in initialState)
            {
                var agentState = ParseAgentState(state.GoalAttribute, state.DecisionAttribute);

                var variablesCollection = parameters.AgentVariables.Cast<IVariable>().ToList();
                agentState.PrivateVariables = ParseAgentVariables(variablesCollection, state.GoalAttribute.Agent);

                parsedStates.Add(agentState);
            }

            return new InitialStateConfiguration
            {
                AgentsState = parsedStates.ToArray()
            };
        }

        private static AgentStateConfiguration ParseAgentState(AgentGoalAttribute goalAttribute, AgentDecisionOption decisionAttribute)
        {
            var agentState = new AgentStateConfiguration();

            agentState.Name = goalAttribute.Agent;
            agentState.NumberOfAgents = 1;
            agentState.PrototypeOfAgent = goalAttribute.Archetype;
            agentState.AssignedGoals = goalAttribute.Goals.Split('|').ToArray();

            var parsedDecisionOptions = ParseAgentDecisionOptions(decisionAttribute.DecisionOptions);
            agentState.AnticipatedInfluenceState = parsedDecisionOptions;
            agentState.AssignedDecisionOptions = parsedDecisionOptions.Keys.ToArray();

            var parsedGoalState = ParseAgentGoalAttributes(goalAttribute);
            agentState.GoalsState = parsedGoalState;

            return agentState;
        }

        private static Dictionary<string, GoalStateConfiguration> ParseAgentGoalAttributes(AgentGoalAttribute goalAttribute)
        {
            var matchPattern = new Regex(@"(G\d+)<(?:(\d+\.?\d*)|(\w+)|(--))>");
            var matchReferencePattern = new Regex(@"(G\d+)<(\w+|--)>");
            var matchRangePattern = new Regex(@"(G\d+)<(?:(?:(?:(-*\d+\.?\d*)|(\w+));(?:(-*\d+\.?\d*)|(\w+)))|(--))>");

            var result = goalAttribute.Goals.Split('|').ToDictionary(k => k, k => new GoalStateConfiguration());

            var matchedFocalValues = matchPattern.Matches(goalAttribute.GoalFocalValues);

            foreach (Match focalValue in matchedFocalValues)
            {
                var goal = focalValue.Groups[1].Value;
                var goalStateConfiguration = result[goal];

                if (focalValue.Groups[4].Value != IgnoredValue)
                {
                    var focalReference = focalValue.Groups[3].Value;
                    if (!string.IsNullOrEmpty(focalReference))
                        goalStateConfiguration.FocalValueReference = focalReference;

                    var focal = focalValue.Groups[2].Value;
                    if (!string.IsNullOrEmpty(focal))
                        goalStateConfiguration.FocalValue = ParseValue<double>(focal);
                }
            }

            var matchedImportances = matchPattern.Matches(goalAttribute.GoalImportance);

            foreach (Match importance in matchedImportances)
            {
                var goal = importance.Groups[1].Value;
                var value = ParseValue<double>(importance.Groups[2].Value);

                result[goal].Importance = value;
            }

            var matchedValueRanges = matchRangePattern.Matches(goalAttribute.GoalValueRange);

            foreach (Match range in matchedValueRanges)
            {
                var goal = range.Groups[1].Value;

                if (range.Groups[6].Value != IgnoredValue)
                {
                    var state = result[goal];

                    if (!string.IsNullOrEmpty(range.Groups[2].Value))
                    {
                        var minValue = ParseValue<double>(range.Groups[2].Value);
                        state.MinValue = minValue;
                    }

                    if (!string.IsNullOrEmpty(range.Groups[3].Value))
                    {
                        state.MinValueReference = range.Groups[3].Value;
                    }

                    if (!string.IsNullOrEmpty(range.Groups[4].Value))
                    {
                        var maxValue = ParseValue<double>(range.Groups[4].Value);
                        state.MaxValue = maxValue;
                    }

                    if (!string.IsNullOrEmpty(range.Groups[5].Value))
                    {
                        state.MaxValueReference = range.Groups[5].Value;
                    }
                }
            }

            return result;
        }

        private static Dictionary<string, Dictionary<string, double>> ParseAgentDecisionOptions(string decisionOptions)
        {
            var decisionPattern = new Regex(@"(MM\d+-\d+_DO\d+)<((?:G\d+<\d+>&?)+)>");
            var goalPattern = new Regex(@"(G\d+)<(\d+\.?\d*)>");

            var results = new Dictionary<string, Dictionary<string, double>>();

            var matches = decisionPattern.Matches(decisionOptions);

            foreach (Match match in matches)
            {
                var doName = match.Groups[1].Value;
                var goalImportances = match.Groups[2].Value;

                var goalDictionary = new Dictionary<string, double>();

                var goalMatches = goalPattern.Matches(goalImportances);

                foreach (Match goalMatch in goalMatches)
                {
                    var goalName = goalMatch.Groups[1].Value;
                    var goalImprortance = ParseValue<double>(goalMatch.Groups[2].Value);

                    goalDictionary.Add(goalName, goalImprortance);
                }

                results.Add(doName, goalDictionary);
            }

            return results;
        }

        private static Dictionary<string, dynamic> ParseAgentVariables(ICollection<IVariable> configVariables, string key)
        {
            var variables = new Dictionary<string, dynamic>();

            foreach (var variable in configVariables.Where(p => p.Key == key))
            {
                var parsedValue = ParseToDynamicValue(variable.VariableType.Trim(), variable.VariableValue);

                variables.Add(variable.VariableName, parsedValue);
            }

            return variables;
        }

        private static dynamic ParseToDynamicValue(string type, string value)
        {
            var parsedValue = (dynamic)value;

            switch (type)
            {
                case "Integer":
                    parsedValue = ParseValue<int>(value);
                    break;
                case "Double":
                    parsedValue = ParseValue<double>(value);
                    break;
                case "Boolean":
                    parsedValue = ParseValue<bool>(value);
                    break;
            }

            return parsedValue;
        }

        private static Dictionary<string, List<Goal>> ParseGoals(List<GoalAttribute> goalAttributes)
        {
            var dictionary = new Dictionary<string, List<Goal>>();

            foreach (var archetypeGoals in goalAttributes.GroupBy(ga => ga.AgentArchetype))
            {
                var goals = new List<Goal>(goalAttributes.Count);

                foreach (var archetypeGoal in archetypeGoals)
                {
                    var goal = new Goal
                    {
                        Name = archetypeGoal.Name,
                        ReferenceVariable = archetypeGoal.ReferenceVariable,
                        Tendency = archetypeGoal.GoalTendency,
                        IsCumulative = archetypeGoal.IsCumulative,
                        ChangeFocalValueOnPrevious = archetypeGoal.ChangeValueOnPrior
                        //FocalValue =  now it is agent specific value if i'm not mistaken
                    };

                    goals.Add(goal);
                }

                dictionary.Add(archetypeGoals.Key, goals);
            }

            return dictionary;
        }

        private static Dictionary<string, Dictionary<string, MentalModelConfiguration>> ParseMentalModels(List<MentalModel> mentalModels)
        {
            var dictionary = new Dictionary<string, Dictionary<string, MentalModelConfiguration>>();

            foreach (var archetypeMentalModels in mentalModels.GroupBy(m => m.AgentArchetype))
            {
                var archetypeMentalModelsDictionary = new Dictionary<string, MentalModelConfiguration>();

                foreach (var mentalModel in archetypeMentalModels)
                {
                    var parsedName = ParseMentalModelName(mentalModel.Name);

                    MentalModelConfiguration configuration;
                    if (!archetypeMentalModelsDictionary.TryGetValue(parsedName.MentalModel.ToString(), out configuration))
                    {
                        configuration = new MentalModelConfiguration
                        {
                            Name = mentalModel.Name,
                            AssociatedWith = mentalModel.AssociatedWithGoals.Split('|'),
                            Layer = new Dictionary<string, DecisionOptionLayerConfiguration>()
                        };

                        archetypeMentalModelsDictionary[parsedName.MentalModel.ToString()] = configuration;
                    }

                    int round;
                    int.TryParse(mentalModel.ConsequentRound, out round);

                    var layer = new DecisionOptionLayerConfiguration
                    {
                        ConsequentPrecisionDigitsAfterDecimalPoint = round,
                        ConsequentValueInterval = ParseRange<double>(mentalModel.ConsequentValueRange),
                        Modifiable = mentalModel.Modifiable,
                        MaxNumberOfDecisionOptions = mentalModel.MaxNumberOfDesignOptions,
                        ConsequentRelationshipSign = ParseComplexValue(mentalModel.DesignOptionGoalRelationship).ToDictionary(v => v.Key, v => v.Value)
                    };

                    configuration.Layer[parsedName.MentalSubModel.ToString()] = layer;
                }

                dictionary[archetypeMentalModels.Key] = archetypeMentalModelsDictionary;
            }

            return dictionary;
        }

        private static (int MentalModel, int MentalSubModel) ParseMentalModelName(string mentalModelString)
        {
            var regex = new Regex(@"^MM(\d{1,})-(\d{1,})$");

            var match = regex.Match(mentalModelString);

            if (!match.Success && match.Groups.Count < 3)
                throw new ArgumentException("Mental model name is not valid");

            var mentalModel = int.Parse(match.Groups[1].Value);
            var mentalSubModel = int.Parse(match.Groups[2].Value);

            return (mentalModel, mentalSubModel);
        }

        private static (int MentalModel, int MentalSubModel, int DecisionOptionNumber) ParseDecisionOptionName(string decisionOptionName)
        {
            var regex = new Regex(@"^MM(\d{1,})-(\d{1,})_DO(\d{1,})$");

            var match = regex.Match(decisionOptionName);

            if (!match.Success && match.Groups.Count < 4)
                throw new ArgumentException("Decision option name is not valid");

            var mentalModel = int.Parse(match.Groups[1].Value);
            var mentalSubModel = int.Parse(match.Groups[2].Value);
            var decisionOptionNumber = int.Parse(match.Groups[3].Value);

            return (mentalModel, mentalSubModel, decisionOptionNumber);
        }

        private static string GetDecisionOptionMentalModelName(string decisionOptionName)
        {
            var parts = decisionOptionName.Split('_');

            if (parts.Length < 2)
                throw new ArgumentException("Decision option name is not valid");

            return parts[0];
        }

        private static T GetValueByName<T>(string key, string @string)
        {
            var list = @string.Split('|');

            var pair = list.FirstOrDefault(p => p.Contains(key));

            if (pair == null)
                return default(T);

            var stringValue = pair.Remove(0, key.Length);
            stringValue = stringValue.Remove(0, 1);
            stringValue = stringValue.Remove(stringValue.Length - 1, 1);

            return ParseValue<T>(stringValue);
        }

        private static T ParseValue<T>(string stringValue)
        {
            return (T)Convert.ChangeType(stringValue, typeof(T));
        }

        private static T[] ParseRange<T>(string value)
        {
            if (string.IsNullOrEmpty(value))
                return new T[] { };

            var regex = new Regex(@"^(?<begin>\d+(\.\d+)?)-(?<end>\d+(\.\d+)?)$");

            var match = regex.Match(value);

            try
            {
                var values = new[] { match.Groups["begin"].Value, match.Groups["end"].Value };
                return values.Select(v => (T)Convert.ChangeType(v, typeof(T))).ToArray();
            }
            catch (Exception)
            {
                throw new ArgumentException("Not valid range string");
            }
        }

        private static List<(string Key, string Value)> ParseComplexValue(string value)
        {
            var result = new List<(string Key, string Value)>();

            if (value == "--")
                return result;

            var regex = new Regex(@"(.+)<([+-]+)>");
            var pairs = value.Split('|');

            foreach (var pair in pairs)
            {
                var match = regex.Match(pair);

                if (match.Success)
                {
                    var key = match.Groups[1].Value;
                    var val = match.Groups[2].Value;

                    result.Add((key, val));
                }
            }

            return result;
        }
    }
}
