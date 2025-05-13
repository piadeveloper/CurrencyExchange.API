using CurrencyExchange.API.Providers;
using CurrencyExchange.API.Models;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CurrencyExchange.Unit.Tests
{
    public class FrankfurterProviderTests
    {
        private readonly Mock<HttpMessageHandler> _httpMessageHandler;
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly FrankfurterProvider _provider;

        public FrankfurterProviderTests()
        {
            _httpMessageHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandler.Object) { BaseAddress = new Uri("https://api.frankfurter.app/") };
            _cache = new MemoryCache(new MemoryCacheOptions());
            _provider = new FrankfurterProvider(_httpClient, _cache);
        }

        [Fact]
        public async Task GetLatestRatesAsync_ShouldReturnCachedData_WhenDataIsCached()
        {
            var testData = new CurrencyRatesResponse
            {
                BaseCurrency = "EUR",
                Amount = 1,
                Date = DateTime.Now,
                Rates = new Dictionary<string, decimal> { { "USD", 1.1m }, { "GBP", 0.85m } }
            };

            var cacheKey = "latest_EUR_1";

            _cache.Set(cacheKey, testData, TimeSpan.FromMinutes(10));

            var result = await _provider.GetLatestRatesAsync("EUR", 1);
            Assert.Equal(testData.BaseCurrency, result.BaseCurrency);
            Assert.Equal(testData.Amount, result.Amount);
            Assert.Equal(testData.Rates.Count, result.Rates.Count);
        }

        [Fact]
        public async Task GetHistoricalRatesAsync_ShouldReturnCachedData_WhenDataIsCached()
        {
            var dateInThePast = DateTime.Now.AddDays(-50);

            var testData = new HistoricalRatesResponse
            {
                BaseCurrency = "EUR",
                Amount = 1,
                Date = dateInThePast,
                Rates = new Dictionary<string, decimal> { { "USD", 1.1m }, { "GBP", 0.85m } }
            };

            var cacheKey = $"historical_{dateInThePast:yyyy-MM-dd}_{testData.BaseCurrency}_{testData.Amount}_1_20";

            _cache.Set(cacheKey, testData, TimeSpan.FromMinutes(10));

            var result = await _provider.GetHistoricalRatesAsync(dateInThePast, testData.BaseCurrency, testData.Amount, 1, 20);
            Assert.Equal(testData.BaseCurrency, result.BaseCurrency);
            Assert.Equal(testData.Amount, result.Amount);
            Assert.Equal(testData.Rates.Count, result.Rates.Count);
        }

        [Fact]
        public async Task GetTimelineRatesAsync_ShouldReturnCachedData_WhenDataIsCached()
        {
            var startDateInThePast = DateTime.Now.AddDays(-50);
            var endDateInThePast = DateTime.Now.AddDays(-30);

            var testData = new TimeSeriesResponse
            {
                BaseCurrency = "EUR",
                Amount = 1,
                StartDate = startDateInThePast,
                EndDate = endDateInThePast,
                Rates = new Dictionary<DateTime, Dictionary<string, decimal>>
                {
                    { startDateInThePast, new Dictionary<string, decimal> { { "USD", 1.1m }, { "GBP", 0.85m } } },
                    { endDateInThePast, new Dictionary<string, decimal> { { "USD", 1.2m }, { "GBP", 0.87m } } }
                }
            };

            string cacheKey = $"timeseries_{startDateInThePast:yyyy-MM-dd}_{endDateInThePast:yyyy-MM-dd}_{testData.BaseCurrency}_{testData.Amount}_1_20";

            _cache.Set(cacheKey, testData, TimeSpan.FromMinutes(10));

            var result = await _provider.GetTimeSeriesDataAsync(startDateInThePast, endDateInThePast, testData.BaseCurrency, testData.Amount, 1, 20);
            Assert.Equal(testData.BaseCurrency, result.BaseCurrency);
            Assert.Equal(testData.Amount, result.Amount);
            Assert.Equal(testData.Rates.Count, result.Rates.Count);
        }


    }
}