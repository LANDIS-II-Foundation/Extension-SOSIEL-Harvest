using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Landis.Library.HarvestManagement;
using Landis.Utilities.PlugIns;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Landis.Extension.SOSIELHarvest.Helpers
{
    public static class PrescriptionExtension
    {
        private static readonly JsonSerializer Serializer;

        static PrescriptionExtension()
        {
            Serializer = new JsonSerializer
            {
                Context = new StreamingContext(StreamingContextStates.All),
                TypeNameHandling = TypeNameHandling.All,
                PreserveReferencesHandling = PreserveReferencesHandling.All,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                Formatting = Formatting.Indented
            };
        }

        public static Prescription Copy(this Prescription prescription)
        {
            return DeepClone(prescription);
        }

        private static T DeepClone<T>(T original)
        {
            var writer = new StringWriter();

            Serializer.Serialize(new JsonTextWriter(writer), original);

            var @string = writer.ToString();

            return Serializer.Deserialize<T>(new JsonTextReader(new StringReader(@string)));
        }
    }

    public class AllFieldsContractResolver : DefaultContractResolver
    {
        //protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        //{
        //    var props = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
        //            .Select(f => base.CreateProperty(f, memberSerialization)).ToList();
        //    props.ForEach(p => { p.Writable = true; p.Readable = true; });
        //    return props;
        //}

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var props = type
                .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                .Select(p => base.CreateProperty(p, memberSerialization))
                .Union(type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                    .Select(f => base.CreateProperty(f, memberSerialization)))
                .ToList();
            props.ForEach(p => { p.Writable = true; p.Readable = true; });
            return props;
        }
    }
}