using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Unity.ReferenceProject.Persistence
{
    public class QuaternionConverter : JsonConverter
    {
        /// <summary>
        /// Overrides the write functions to intercept the values and convert to an array for our serialization.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="serializer"></param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Quaternion coords = (Quaternion) value;
            JArray values = new JArray {coords.x, coords.y, coords.z, coords.w};
            serializer.Serialize(writer, values);
        }

        /// <summary>
        /// Overrides the read functions to intercept the values as an array and convert array entries to Quaternion.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="objectType"></param>
        /// <param name="existingValue"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            try
            {
                JArray coords = JArray.Parse(JToken.Load(reader).ToString());
                List<float> values = coords.ToObject<List<float>>();
                if (values.Count != 4)
                    return null;

                return new Quaternion(values[0], values[1], values[2], values[3]);
            }
            catch (Exception e)
            {
                Debug.LogError($"[QuaternionConverter] Error parsing Quaternion: {e}");
            }

            return null;
        }

        /// <summary>
        /// Override to return true if the object type is Quaternion.
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Quaternion);
        }
    }
}