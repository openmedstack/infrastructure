namespace OpenMedStack.Infrastructure.Bootstrapping.Json
{
    using System;
    using System.Linq;
    using System.Security.Claims;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class ClaimConverter : JsonConverter<Claim>
    {
        public override void WriteJson(JsonWriter writer, Claim? value, JsonSerializer serializer)
        {
            if (value is null)
            {
                return;
            }
            writer.WriteStartObject();
            writer.WritePropertyName(value.Type);
            writer.WriteValue(value.Value);
            writer.WriteEndObject();
        }

        public override Claim ReadJson(
            JsonReader reader,
            Type objectType,
            Claim? existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            var obj = serializer.Deserialize<JObject>(reader);
            var properties = obj!.Properties().ToArray();
            if (properties.Length == 1)
            {
                var type = obj.Properties().First().Name;
                var value = obj[type];
                return new Claim(type, value!.ToObject<string>()!);
            }

            return new Claim(
                obj["type"]!.ToObject<string>()!,
                obj["value"]!.ToObject<string>()!,
                obj["valueType"]!.ToObject<string>(),
                obj["issuer"]!.ToObject<string>());
        }
    }
}
