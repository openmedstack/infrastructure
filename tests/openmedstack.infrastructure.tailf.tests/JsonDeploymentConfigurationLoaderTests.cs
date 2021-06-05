namespace OpenMedStack.Infrastructure.Tailf.Tests
{
    using System;
    using Bootstrapping;
    using OpenMedStack;
    using Xunit;

    public class JsonDeploymentConfigurationLoaderTests
    {
        [Fact]
        public void CanSubstituteWithEnvironmentVariables()
        {
            Environment.SetEnvironmentVariable("test", "test", EnvironmentVariableTarget.Process);

            var configuration = "deployment.manifest".Load<DeploymentConfiguration>();

            Assert.Equal("test", configuration.TokenService);

            Environment.SetEnvironmentVariable("test", null, EnvironmentVariableTarget.Process);

        }
    }
}
