// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ServiceDictionaryConverter.cs" company="Reimers.dk">
//   Copyright © Reimers.dk
// </copyright>
// <summary>
//   Defines the ServiceDictionaryConverter type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenMedStack.Infrastructure.Bootstrapping.Json
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Text.RegularExpressions;
    using Newtonsoft.Json;

    public class ServiceDictionaryConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => typeof(IDictionary<Regex, Uri>).IsAssignableFrom(objectType);

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var dictionary = existingValue as IDictionary<Regex, Uri> ?? new Dictionary<Regex, Uri>();
            reader.Read();
            while (reader.TokenType != JsonToken.EndObject)
            {
                var key = new Regex((string)reader.Value!, RegexOptions.Compiled);
                var uri = reader.ReadAsString() ?? throw new SerializationException(
                    $"Could not read json value for {key}");
                var value = new Uri(uri);
                dictionary.Add(key, value);
                reader.Read();
            }

            return dictionary;
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value is null)
            {
                writer.WriteNull();
                return;
            }
            var dictionary = (IDictionary<Regex, Uri>)value;
            writer.WriteStartObject();
            foreach (var entry in dictionary)
            {
                writer.WritePropertyName(entry.Key.ToString());
                writer.WriteValue(entry.Value.ToString());
            }

            writer.WriteEndObject();
        }
    }
}