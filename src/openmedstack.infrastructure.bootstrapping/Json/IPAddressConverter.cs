// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPAddressConverter.cs" company="Reimers.dk">
//   Copyright © Reimers.dk
// </copyright>
// <summary>
//   Defines the IpAddressConverter type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenMedStack.Infrastructure.Bootstrapping.Json
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class IpAddressConverter : JsonConverter
    {
        /// <inheritdoc />
        public override bool CanConvert(Type objectType) => objectType == typeof(IPAddress)
                                                            || typeof(IEnumerable<IPAddress>).IsAssignableFrom(objectType);

        /// <inheritdoc />
        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            // convert an ipaddress represented as a string into an IPAddress object and return it to the caller
            if (objectType == typeof(IPAddress))
            {
                return IPAddress.Parse(JToken.Load(reader).ToString());
            }

            // convert an array of IPAddresses represented as strings into a List<IPAddress> object and return it to the caller
            if (typeof(IEnumerable<IPAddress>).IsAssignableFrom(objectType))
            {
                return JToken.Load(reader).Select(address => IPAddress.Parse((string)address!)).ToList();
            }

            throw new NotSupportedException($"Cannot read {existingValue}");
        }

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value is null)
            {
                writer.WriteNull();
                return;
            }
            // convert an IPAddress object to a string representation of itself and Write it to the serializer
            if (value.GetType() == typeof(IPAddress))
            {
                JToken.FromObject(value.ToString()!).WriteTo(writer);
                return;
            }

            // convert a List<IPAddress> object to a string[] representation of itself and Write it to the serializer
            if (value is IEnumerable<IPAddress> addresses)
            {
                JToken.FromObject((from n in addresses select n.ToString()).ToArray()).WriteTo(writer);
                return;
            }

            throw new NotSupportedException($"Cannot serialize {value}");
        }
    }
}