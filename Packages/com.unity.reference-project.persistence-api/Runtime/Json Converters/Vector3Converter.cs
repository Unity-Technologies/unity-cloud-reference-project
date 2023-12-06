using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Unity.ReferenceProject.Persistence
{
    public class Vector3Converter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Vector3 coords = (Vector3) value;
            JArray values = new JArray {coords.x, coords.y, coords.z};
            serializer.Serialize(writer, values);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            try
            {
                JArray coords = JArray.Parse(JToken.Load(reader).ToString());
                List<float> values = coords.ToObject<List<float>>();
                if (values.Count > 0)
                    return new Vector3(values[0], values[1], values[2]);

                return existingValue;
            }
            catch (Exception e)
            {
                Debug.Log($"[Vector3Converter] Error parsing Vector3, using Unity methods: {e}");
                return existingValue;
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Vector3);
        }
    }
}