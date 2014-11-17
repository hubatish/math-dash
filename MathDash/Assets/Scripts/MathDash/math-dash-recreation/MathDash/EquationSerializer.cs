using Newtonsoft.Json;
using System;
using UnityEngine;

namespace MathDash
{
    public class WierdNameSerializer : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(Equation).IsAssignableFrom(objectType);
        }
    }
}

