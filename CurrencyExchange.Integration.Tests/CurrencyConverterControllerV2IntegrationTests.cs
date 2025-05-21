
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CurrencyExchange.API.Models;
using CurrencyExchange.Integration.Tests;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyExchange.API.IntegrationTests
{
    public class CurrencyConverterV2Tests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public CurrencyConverterV2Tests(CustomWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        private async Task AuthenticateAsync()
        {
            var username = "testuser";
            var userSecret = "secret";
            var response = await _client.PostAsync($"/api/v1/auth/login?username={username}&userSecret={userSecret}", null);
            response.EnsureSuccessStatusCode();

            var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResponse.Token);
        }


        [Fact]
        public async Task Post_LatestRates_ShouldReturnSuccess()
        {
            await AuthenticateAsync();

            var request = new LatestRatesRequest
            {
                Provider = "Frankfurter",
                BaseCurrency = "EUR",
                Amount = 100
            };

            var response = await _client.PostAsJsonAsync("/api/v2/CurrencyConverter/latest", request);

            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<CurrencyRatesResponse>();

            Assert.NotNull(result);
            Assert.NotEmpty(result.Rates);
        }

        [Fact]
        public async Task Post_LatestRates_WithUnsupportedCurrency_ShouldReturnBadRequest()
        {
            await AuthenticateAsync();

            var request = new LatestRatesRequest
            {
                Provider = "Frankfurter",
                BaseCurrency = "XYZ",
                Amount = 100
            };

            var response = await _client.PostAsJsonAsync("/api/v2/CurrencyConverter/latest", request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var details = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            Assert.Contains("not supported", details?.Detail, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task Post_LatestRates_WhenProviderReturnsError_ShouldReturnBadRequest()
        {
            await AuthenticateAsync();

            var request = new LatestRatesRequest
            {
                Provider = "Frankfurter",
                BaseCurrency = "INVALID",
                Amount = 100
            };

            var response = await _client.PostAsJsonAsync("/api/v2/CurrencyConverter/latest", request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Post_LatestRates_WithNegativeAmount_ShouldReturnBadRequest()
        {
            await AuthenticateAsync();

            var request = new LatestRatesRequest
            {
                Provider = "Frankfurter",
                BaseCurrency = "EUR",
                Amount = -5
            };

            var response = await _client.PostAsJsonAsync("/api/v2/CurrencyConverter/latest", request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            Assert.Contains("greater than zero", problem?.Extensions["errors"]?.ToString() ?? "");
        }

        [Fact]
        public async Task Post_HistoricalRates_ShouldReturnSuccess()
        {
            await AuthenticateAsync();

            var request = new HistoricalRatesRequest
            {
                Date = DateTime.UtcNow.AddDays(-10),
                Provider = "Frankfurter",
                BaseCurrency = "EUR",
                Amount = 50,
                Page = 1,
                PageSize = 10
            };

            
            var response = await _client.PostAsJsonAsync($"/api/v2/CurrencyConverter/historical", request);

            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<HistoricalRatesResponse>();

            Assert.NotNull(result);
            Assert.NotEmpty(result.Rates);
        }


        [Fact]
        public async Task Post_HistoricalRates_WithFutureDate_ShouldReturnBadRequest()
        {
            await AuthenticateAsync();

            var futureDate = DateTime.UtcNow.AddDays(10);

            var request = new HistoricalRatesRequest
            {
                Date = futureDate,
                Provider = "Frankfurter",
                BaseCurrency = "EUR",
                Amount = 10,
                Page = 1,
                PageSize = 10
            };

            var response = await _client.PostAsJsonAsync($"/api/v2/CurrencyConverter/historical", request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Post_TimeSeries_ShouldReturnSuccess()
        {
            await AuthenticateAsync();

            var request = new TimeSeriesRequest
            {
                Provider = "Frankfurter",
                BaseCurrency = "EUR",
                Amount = 10,
                StartDate = DateTime.UtcNow.AddDays(-7),
                EndDate = DateTime.UtcNow,
                Page = 1,
                PageSize = 10
            };

            var response = await _client.PostAsJsonAsync("/api/v2/CurrencyConverter/timeseries", request);

            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<TimeSeriesResponse>();

            Assert.NotNull(result);
            Assert.NotEmpty(result.Rates);
        }

        [Fact]
        public async Task Post_TimeSeries_WithInvalidDateRange_ShouldReturnBadRequest()
        {
            await AuthenticateAsync();

            var request = new TimeSeriesRequest
            {
                Provider = "Frankfurter",
                BaseCurrency = "EUR",
                Amount = 10,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(-10),
                Page = 1,
                PageSize = 10
            };

            var response = await _client.PostAsJsonAsync("/api/v2/CurrencyConverter/timeseries", request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            Assert.Contains("Start date", problem?.Detail ?? "");
        }

        [Fact]
        public async Task Post_TimeSeries_WithZeroPage_ShouldReturnBadRequest()
        {
            await AuthenticateAsync();

            var request = new TimeSeriesRequest
            {
                Provider = "Frankfurter",
                BaseCurrency = "EUR",
                Amount = 10,
                StartDate = DateTime.UtcNow.AddDays(-10),
                EndDate = DateTime.UtcNow,
                Page = 0, // Неверное значение
                PageSize = 10
            };

            var response = await _client.PostAsJsonAsync("/api/v2/CurrencyConverter/timeseries", request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Post_Currencies_ShouldReturnSuccess()
        {
            await AuthenticateAsync();

            var request = new CurrenciesRequest { Provider = "Frankfurter" };

            var response = await _client.PostAsJsonAsync("/api/v2/CurrencyConverter/currencies", request);

            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<CurrenciesResponse>();

            Assert.NotNull(result);
            Assert.NotEmpty(result.Keys);
        }

        [Theory]
        [InlineData(0, 10)]
        [InlineData(1, 0)]
        [InlineData(0, 0)]
        public async Task Post_TimeSeries_WithInvalidPagination_ShouldReturnBadRequest(int page, int pageSize)
        {
            await AuthenticateAsync();

            var request = new TimeSeriesRequest
            {
                Provider = "Frankfurter",
                BaseCurrency = "EUR",
                Amount = 10,
                StartDate = DateTime.UtcNow.AddDays(-10),
                EndDate = DateTime.UtcNow,
                Page = page,
                PageSize = pageSize
            };

            var response = await _client.PostAsJsonAsync("/api/v2/CurrencyConverter/timeseries", request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Post_LatestRates_Unauthorized_IfTokenMissing()
        {
            var request = new LatestRatesRequest
            {
                Provider = "Frankfurter",
                BaseCurrency = "EUR",
                Amount = 100
            };

            var response = await _client.PostAsJsonAsync("/api/v2/CurrencyConverter/currencies", request);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
