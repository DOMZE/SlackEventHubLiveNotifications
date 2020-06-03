using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace LiveNotificationFunction
{
    public class KeyvaultEventDataConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.String)
            {
                throw new JsonException("only string are available");
            }

            var item = JObject.Parse(reader.Value.ToString());
            return item.ToObject<KeyvaultEventData>();
        }

        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override bool CanWrite { get; } = false;
    }
}