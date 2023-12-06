
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Unity.Geospatial.Unit.Tests
{
    [TestFixture]
    public class TestsSerialize
    {
        private static readonly object[][] GetConstructors =
        {
            new object[] {5.0, 1, Si.Centimeter},
            new object[] {32.5, 2, Si.Centimeter2},
            new object[] {-64.44, 3, Si.Centimeter3},
            new object[] {1.98, 1, Si.Becquerel},
            new object[] {0.85, 2, Si.Becquerel},
            new object[] {0.23, 3, Si.Becquerel},
        };

        [TestCaseSource(nameof(GetConstructors))]
        public void Binary(double value, int power, IUnitDef unitDef)
        {
            Unit before = new Unit(value, unitDef, (byte) power);

            IFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            formatter.Serialize(stream, before);

            string encoded = Convert.ToBase64String(stream.ToArray());
            stream.Close();

            byte[] decoded = Convert.FromBase64String(encoded);
            stream = new MemoryStream(decoded);
            object after = formatter.Deserialize(stream);
            stream.Close();
            
            Assert.AreEqual(before, after);
        }
        
        [TestCaseSource(nameof(GetConstructors))]
        public void Json(double value, int power, IUnitDef unitDef)
        {
            Unit before = new Unit(value, unitDef, (byte)power);
            
            JsonSerializer serializer = JsonSerializer.Create(Serializer.GetDefaultSettings());
            
            StringBuilder builder = new StringBuilder();

            using StringWriter stringWriter = new StringWriter(builder);
            using JsonTextWriter writer = new JsonTextWriter(stringWriter);
            serializer.Serialize(writer, before);

            writer.Flush();
            string text = builder.ToString();

            using StringReader stringReader = new StringReader(text);
            using JsonTextReader reader = new JsonTextReader(stringReader);
            object after = serializer.Deserialize<Unit>(reader);
            
            Assert.AreEqual(before, after);
        }
    }
    
    public class Serializer : JsonSerializer
    {
        private static readonly JsonConverterCollection JsonConverters = new()
        {
            new Json.UnitConverter()
        };

        public override JsonConverterCollection Converters =>
            JsonConverters;

        public static JsonSerializerSettings GetDefaultSettings() =>
            new()
            {
                Converters = JsonConverters
            };
    }
}
