
using System;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace Unity.Geospatial.Unit.Json
{
    /// <summary>
    /// Converts a <see cref="Unit"/> to or from JSON.
    /// </summary>
    public class UnitConverter : JsonConverter
    {
        /// <summary>
        /// Determines whether the specified type can be converted.
        /// </summary>
        /// <param name="objectType">The type to compare against.</param>
        /// <returns>
        /// true if the type can be converted;
        /// otherwise, false.
        /// </returns>
        public override bool CanConvert(Type objectType) =>
            typeof(Unit) == objectType;

        /// <summary>
        /// Reads and converts the JSON to a <see cref="Unit"/> instance.
        /// </summary>
        /// <param name="reader">The JsonReader to read from.</param>
        /// <param name="objectType">Type of the object to convert to.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The object value deserialized.</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) =>
            reader.TokenType == JsonToken.Null 
                ? Misc.Null 
                : new Unit(serializer.Deserialize<Dictionary<string, object>>(reader));

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The JsonWriter to write to.</param>
        /// <param name="value">The value requested to be serialized.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Unit unit = (Unit)value;

            if (unit.IsNull)
            {
                writer.WriteNull();
                return;
            }
            
            Dictionary<string, object> data = new Dictionary<string, object>()
            {
                { "type", "UnityGeospatial.Unit" },
                { "value", unit.Value },
                { "power", (int)unit.Power },
                { "unit_name", unit.UnitDef.Name },
                { "unit_type", unit.UnitDef.GetType().Name },
            };

            serializer.Serialize(writer, data);
        }
    }
}
