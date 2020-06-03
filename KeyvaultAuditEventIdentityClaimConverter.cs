using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace LiveNotificationFunction
{
    public class KeyvaultAuditEventIdentityClaimConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.StartObject)
            {
                throw new JsonException("The type is not an object");
            }

            var item = JObject.Load(reader);
            var claims = new List<Claim>();

            if (item.Type == JTokenType.Object)
            {
                foreach (var (key, value) in item)
                {
                    var claim = new Claim(key, value.Value<string>());
                    claims.Add(claim);
                }
            }

            return claims;
        }

        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override bool CanWrite { get; } = false;
    }
}