namespace OpenMedStack.Bootstrapping.Infrastructure.Tests
{
    using System;
    using System.Security.Claims;
    using OpenMedStack.Bootstrapping.Infrastructure.Infrastructure;
    using OpenMedStack.Bootstrapping.Infrastructure.Json;
    using Newtonsoft.Json;
    using Xunit;

    public class JsonDeploymentConfiguratinLoaderTests
    {
        [Fact]
        public void CanSubstituteWithEnvironmentVariables()
        {
            Environment.SetEnvironmentVariable("test", "test", EnvironmentVariableTarget.Process);

            var args = new DeviceArgs { ConfigFile = "deployment.manifest" };
            var configuration = args.Load<DeploymentConfiguration>();

            Assert.Equal("test", configuration.TokenService);

            Environment.SetEnvironmentVariable("test", null, EnvironmentVariableTarget.Process);

        }
    }

    public class SerializationTests
    {
        [Fact]
        public void CanSerialize()
        {
            var claim = new Claim("sub", "test", ClaimValueTypes.String, "someone", "someone_else");
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new ClaimConverter());
            var json = JsonConvert.SerializeObject(claim, settings);

            var deserialized = JsonConvert.DeserializeObject<Claim>(json, settings);

            Assert.Equal(claim.Type, deserialized!.Type);
        }
    }
}
