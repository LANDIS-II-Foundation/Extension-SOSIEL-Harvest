using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
                    MakeAlgorithmConfiguration(parameters.Demographic, parameters.Probabilities),
                AgentConfiguration = MakeAgentConfiguration(parameters),
                InitialState = MakeInitialStateConfiguration()
            };

            return configuration;
        }

        private static AlgorithmConfiguration MakeAlgorithmConfiguration(Demographic demographic, IEnumerable<Probability> probabilities)
        {
            return new AlgorithmConfiguration
            {
                CognitiveLevel = CognitiveLevel.CL4,
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

            var agents = parameters.AgentGoalAttributes.Select(ag => ag.Agent).Distinct();

            foreach (var agent in agents)
            {
                var agentArchetype = new AgentArchetype();

                foreach (var goal in ParseAgentGoals(parameters.AgentGoalAttributes.First(ag=> ag.Agent.Equals(agent))))
                    agentArchetype.Goals.Add(goal);

                foreach (var agentVariable in ParseAgentVariables(parameters.AgentVariables, agent))
                    agentArchetype.CommonVariables.Add(agentVariable.Key, agentVariable.Value);
            }

            return agentArchetypes;
        }

        private static InitialStateConfiguration MakeInitialStateConfiguration()
        {
            throw new NotImplementedException();
        }

        private static Dictionary<string, dynamic> ParseAgentVariables(List<AgentVariable> sosielParametersAgentVariables, string agent)
        {
            var variables = new Dictionary<string, dynamic>();

            foreach (var variable in sosielParametersAgentVariables.Where(p => p.Agent == agent))
            {
                var parsedValue = default(dynamic);

                switch (variable.VariableType)
                {
                    case "Integer":
                        parsedValue = int.Parse(variable.VariableValue);
                        break;
                }

                variables.Add(variable.VariableName, parsedValue);
            }

            return variables;
        }

        private static List<Goal> ParseAgentGoals(AgentGoalAttribute agentGoalAttribute)
        {
            var goals = new List<Goal>();

            var goalNames = agentGoalAttribute.Goals.Split('|');

            foreach (var goalName in goalNames)
            {
                var goal = new Goal
                {
                    Name = goalName,
                    FocalValue = GetValueByName<double>(goalName, agentGoalAttribute.GoalFocalValues),
                    FocalValueReference = GetValueByName<string>(goalName, agentGoalAttribute.GoalFocalValueReference),
                };

                goals.Add(goal);
            }

            return goals;
        }

        private static T GetValueByName<T>(string key, string @string)
        {
            var list = @string.Split('|');

            var pair = list.FirstOrDefault(p => p.Contains(key));

            if (pair == null)
                return default;

            var stringValue = pair.Remove(0, key.Length);
            stringValue = stringValue.Remove(0, 1);
            stringValue = stringValue.Remove(stringValue.Length - 1, 1);

            return (T)Convert.ChangeType(stringValue, typeof(T));
        }
    }
}
