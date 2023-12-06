using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Unity.ReferenceProject.MeasureTool
{
    public class AnchorConverter : JsonConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            var jsonObject = JObject.Load(reader);

            var positionToken = jsonObject["Position"];
            if (positionToken == null)
            {
                throw new JsonSerializationException("Missing 'position' property in JSON for IAnchor object.");
            }
        
            var position = positionToken.ToObject<Vector3>(serializer);

            var normalToken = jsonObject["Normal"];
            if (normalToken == null)
            {
                throw new JsonSerializationException("Missing 'normal' property in JSON for IAnchor object.");
            }
            var normal = normalToken.ToObject<Vector3>(serializer);

            return new PointAnchor(position, normal);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var anchor = (IAnchor)value;

            writer.WriteStartObject();

            writer.WritePropertyName("Position");
            serializer.Serialize(writer, anchor.Position);

            writer.WritePropertyName("Normal");
            serializer.Serialize(writer, anchor.Normal);

            writer.WriteEndObject();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IAnchor);
        }
    }
}
