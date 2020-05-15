// Can use classes from the listed namespaces.
using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

/// The container for classes and other namespaces.
namespace Landis.Extension.SOSIELHuman.Configuration
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
    }
}
/*
Copyright 2020 Garry Sotnik

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/
