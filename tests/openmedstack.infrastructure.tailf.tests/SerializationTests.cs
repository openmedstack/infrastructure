namespace OpenMedStack.Infrastructure.Tailf.Tests
{
    using System.Security.Claims;
    using Bootstrapping.Json;
    using Newtonsoft.Json;
    using Xunit;

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