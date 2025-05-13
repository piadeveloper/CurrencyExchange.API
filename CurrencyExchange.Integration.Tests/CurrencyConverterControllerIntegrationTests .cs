using CurrencyExchange.API;
using CurrencyExchange.API.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace CurrencyExchange.Integration.Tests
{
    public class CurrencyConverterControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public CurrencyConverterControllerIntegrationTests(CustomWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        private async Task AuthenticateAsync()
        {
            var username = "testuser";
            var response = await _client.PostAsync($"/api/auth/login?username={username}", null);
            response.EnsureSuccessStatusCode();

            var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResponse.Token);
        }

        /*Start tests for latest*/
        [Fact]
        public async Task GetLatestRates_ShouldReturnOk_WhenCalledWithValidParameters()
        {
            await AuthenticateAsync();

            var response = await _client.GetAsync("/api/currencyconverter/latest?provider=Frankfurter&baseCurrency=EUR&amount=1");
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<CurrencyRatesResponse>();
            Assert.NotNull(result);
            Assert.Equal("EUR", result.BaseCurrency);
            Assert.Equal(1, result.Amount);
        }
        
        [Fact]
        public async Task GetLatestRates_ShouldReturnBadRequest_WhenCalledWithInvalidAmount()
        {
            await AuthenticateAsync();

            var response = await _client.GetAsync("/api/currencyconverter/latest?provider=Frankfurter&baseCurrency=EUR&amount=-1");
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetLatestRates_ShouldReturnBadRequest_WhenCalledWithUnsupportedBaseCurrency()
        {
            await AuthenticateAsync();

            var response = await _client.GetAsync("/api/currencyconverter/latest?provider=Frankfurter&baseCurrency=TRY&amount=1");
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetLatestRates_ShouldReturnUnauthorized_WhenNotAuthenticated()
        {
            // Убираем авторизацию, если была установлена
            _client.DefaultRequestHeaders.Authorization = null;

            var response = await _client.GetAsync("/api/currencyconverter/latest?provider=Frankfurter&baseCurrency=EUR&amount=1");

            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }
        /*End tests for latest*/

        /*Start tests for historical*/
        [Fact]
        public async Task GetHistoricalRates_ShouldReturnOk_WhenCalledWithValidParameters()
        {
            await AuthenticateAsync();

            var date = DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd");
            var response = await _client.GetAsync($"/api/currencyconverter/historical/{date}?provider=Frankfurter&baseCurrency=EUR&amount=1");

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<HistoricalRatesResponse>();
            Assert.NotNull(result);
            Assert.Equal("EUR", result.BaseCurrency);
            Assert.Equal(1, result.Amount);
            Assert.Equal(date, result.Date.ToString("yyyy-MM-dd"));
        }

        [Fact]
        public async Task GetHistoricalRates_ShouldReturnBadRequest_WhenCalledWithInvalidAmount()
        {
            await AuthenticateAsync();

            var date = DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd");
            var response = await _client.GetAsync($"/api/currencyconverter/historical/{date}?provider=Frankfurter&baseCurrency=EUR&amount=-1");

            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetHistoricalRates_ShouldReturnBadRequest_WhenCalledWithUnsupportedBaseCurrency()
        {
            await AuthenticateAsync();

            var date = DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd");
            var response = await _client.GetAsync($"/api/currencyconverter/historical/{date}?provider=Frankfurter&baseCurrency=XYZ&amount=1");

            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetHistoricalRates_ShouldReturnUnauthorized_WhenNotAuthenticated()
        {
            // Убираем авторизацию, если была установлена
            _client.DefaultRequestHeaders.Authorization = null;

            var date = DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd");
            var response = await _client.GetAsync($"/api/currencyconverter/historical/{date}?provider=Frankfurter&baseCurrency=EUR&amount=1");

            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }
        /*End tests for historical*/

        /*Start tests for timeseries*/
        [Fact]
        public async Task GetTimeSeries_ShouldReturnOk_WhenCalledWithValidParameters()
        {
            await AuthenticateAsync();

            var startDate = DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd");
            var endDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

            var response = await _client.GetAsync($"/api/currencyconverter/timeseries/{startDate}/{endDate}?provider=Frankfurter&baseCurrency=EUR&amount=1");
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<TimeSeriesResponse>();
            Assert.NotNull(result);
            Assert.Equal("EUR", result.BaseCurrency);
            Assert.Equal(1, result.Amount);
        }

        [Fact]
        public async Task GetTimeSeries_ShouldReturnBadRequest_WhenCalledWithInvalidAmount()
        {
            await AuthenticateAsync();

            var startDate = DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd");
            var endDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

            var response = await _client.GetAsync($"/api/currencyconverter/timeseries/{startDate}/{endDate}?provider=Frankfurter&baseCurrency=EUR&amount=-1");
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }
        [Fact]
        public async Task GetTimeSeries_ShouldReturnBadRequest_WhenDateIsInvalid()
        {
            await AuthenticateAsync();

            var response = await _client.GetAsync("/api/currencyconverter/timeseries/2025-02-30/2025-03-01?provider=Frankfurter&baseCurrency=EUR&amount=1");
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetTimeSeries_ShouldReturnBadRequest_WhenCalledWithUnsupportedBaseCurrency()
        {
            await AuthenticateAsync();

            var startDate = DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd");
            var endDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

            var response = await _client.GetAsync($"/api/currencyconverter/timeseries/{startDate}/{endDate}?provider=Frankfurter&baseCurrency=TRY&amount=1");
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetTimeSeries_ShouldReturnUnauthorized_WhenNotAuthenticated()
        {
            
            _client.DefaultRequestHeaders.Authorization = null;

            var startDate = DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd");
            var endDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

            var response = await _client.GetAsync($"/api/currencyconverter/timeseries/{startDate}/{endDate}?provider=Frankfurter&baseCurrency=EUR&amount=1");

            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }
        /*End tests for timeseries*/
    }
}
