using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Unity.ReferenceProject.Persistence
{
    public class ColorValueConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Color coords = (Color)value;
            JArray values = new JArray {coords.r, coords.g, coords.b};
       
            serializer.Serialize(writer, values);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            try
            {
                JArray coords = JArray.Parse(JToken.Load(reader).ToString());
                List<float> values = coords.ToObject<List<float>>();
                return new Color(values[0], values[1], values[2]);
            } catch (Exception e)
            {
                Debug.Log($"[ColorValueConverter] Error parsing Color, using Unity methods: {e}");
                return existingValue;
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Color);
        }
    }
}