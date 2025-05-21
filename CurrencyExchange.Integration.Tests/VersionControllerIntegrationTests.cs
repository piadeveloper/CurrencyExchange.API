using CurrencyExchange.API;
using CurrencyExchange.API.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace CurrencyExchange.Integration.Tests
{
    public class VersionControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public VersionControllerIntegrationTests(CustomWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetVersion_ShouldReturnOk_WithValidVersion()
        {
            var response = await _client.GetAsync("/api/v1/version/getversion");
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<VersionResponse>();

            Assert.NotNull(result);
            Assert.NotEqual("unknown", result.Version);
            Assert.Matches(@"^\d+\.\d+\.\d+\.\d+$", result.Version);
        }
        public class VersionResponse
        {
            public string Version { get; set; }
        }
    }
}
