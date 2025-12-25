using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCUtils.SCSaveManager
{
    internal class AbstractObjectConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType.IsSubclassOf(typeof(AbstractPhysicalObject));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }
            if (reader.TokenType != JsonToken.String)
            {
                throw new JsonSerializationException($"[AbstractObjectConverter] find {reader.TokenType}, expected string");
            }
            var str = (string)reader.Value;
            str = str.Trim('"');
            if (SaveStateManager.CurrentWorld.TryGetTarget(out var world))
            {

                return SaveState.AbstractPhysicalObjectFromString(world, str);
            }
            else
            {
                throw new NullReferenceException("[AbstractObjectConverter] world is null");
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {

            writer.WriteValue(value.ToString());
        }
    }
}
