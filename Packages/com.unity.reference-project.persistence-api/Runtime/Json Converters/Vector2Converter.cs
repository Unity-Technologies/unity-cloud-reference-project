using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Unity.ReferenceProject.Persistence
{
    public class Vector2Converter : JsonConverter
    {
        /// <summary>
        /// Overrides the write functions to intercept the values and convert to an array for our serialization.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="serializer"></param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Vector2 coords = (Vector2)value;
            JArray values = new JArray {coords.x, coords.y};
            serializer.Serialize(writer, values);
        }

        /// <summary>
        /// Overrides the read functions to intercept the values as an array and convert array entries to Vector2.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="objectType"></param>
        /// <param name="existingValue"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            try
            {
                JArray coords = JArray.Parse(JToken.Load(reader).ToString());
                List<float> values = coords.ToObject<List<float>>();
                
                var returnVal = values.Count >= 2 ? new Vector2(values[0], values[1]) : Vector2.zero;
                return returnVal;
            } catch (Exception e)
            {
                Debug.Log($"[Vector2Converter] Error parsing Vector2, using Unity methods: {e}");
                return existingValue;
            }
        }

        /// <summary>
        /// Override to return true if the object type is Vector2.
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Vector2);
        }
    }
}