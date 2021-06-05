// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JsonDeploymentConfigurationLoader.cs" company="Reimers.dk">
//   Copyright © Reimers.dk
// </copyright>
// <summary>
//   Defines the JsonDeploymentConfigurationLoader type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenMedStack.Infrastructure.Bootstrapping
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Linq;
    using Json;
    using Newtonsoft.Json;

    public static class JsonDeploymentConfigurationLoader
    {
        public static T Load<T>(this string configFile)
            where T : DeploymentConfiguration, new()
        {
            var filePath = configFile.ToApplicationPath();
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Could not find configuration file at {filePath}");
            }

            var json = Environment.GetEnvironmentVariables()
                .Cast<DictionaryEntry>()
                .Aggregate(
                    File.ReadAllText(filePath),
                    (current, variable) => current.Replace($"{{{variable.Key}}}", variable.Value!.ToString()));
            var configuration = JsonConvert.DeserializeObject<T>(
                json,
                new ServiceDictionaryConverter(),
                new IpAddressConverter());

            if(configuration == null)
            {
                throw new InvalidOperationException("Cannot deserialize input");
            }

            return configuration;
        }
    }
}
