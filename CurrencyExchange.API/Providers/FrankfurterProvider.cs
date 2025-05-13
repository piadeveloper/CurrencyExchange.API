using CurrencyExchange.API.Helpers;
using CurrencyExchange.API.Models;
using CurrencyExchange.API.Providers.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CurrencyExchange.API.Providers
{
    public class FrankfurterProvider: ICurrencyConverterProvider
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly IMemoryCache _cache;

        public FrankfurterProvider(HttpClient httpClient, IMemoryCache cache)
        {
            _httpClient = httpClient;
            
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new DateTimeConverter("yyyy-MM-dd") }
            };

            _cache = cache;
        }
      
        public async Task<CurrencyRatesResponse> GetLatestRatesAsync(
            string baseCurrency = "EUR",
            decimal amount = 1.0m)
        {
            string cacheKey = $"latest_{baseCurrency}_{amount}";

            if (!_cache.TryGetValue(cacheKey, out CurrencyRatesResponse cachedRates))
            {
                cachedRates = await GetAsync<CurrencyRatesResponse>($"latest?from={baseCurrency}&amount={amount}");
                if (cachedRates != null)
                {
                    _cache.Set(cacheKey, cachedRates, TimeSpan.FromMinutes(10));
                }
            }
            var response = new CurrencyRatesResponse
            {
                Amount = amount,
                BaseCurrency = baseCurrency,
                Date = cachedRates.Date,
                Rates = cachedRates.Rates.Where(r => !UnsupportedCurrencies.ExcludedCurrencies.Contains(r.Key.ToUpper())).ToDictionary()
            };
            return response;
        }

        public async Task<HistoricalRatesResponse> GetHistoricalRatesAsync(
            DateTime date,
            string baseCurrency = "EUR",
            decimal amount = 1.0m,
            int page = 1,
            int pageSize = 20)
        {
            string cacheKey = $"historical_{date:yyyy-MM-dd}_{baseCurrency}_{amount}_{page}_{pageSize}";

            if (!_cache.TryGetValue(cacheKey, out HistoricalRatesResponse cachedRates))
            {
                cachedRates = await GetAsync<HistoricalRatesResponse>($"{date:yyyy-MM-dd}?from={baseCurrency}&amount={amount}");
                if(cachedRates != null)
                {
                    _cache.Set(cacheKey, cachedRates, TimeSpan.FromMinutes(10));
                }
            }
            var totalItems = cachedRates.Rates.Count;
            var paginatedRates = cachedRates.Rates.Where(r => !UnsupportedCurrencies.ExcludedCurrencies.Contains(r.Key.ToUpper()))
                .OrderBy(r => r.Key)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToDictionary(r => r.Key, r => r.Value);


            var response = new HistoricalRatesResponse
            {
                Amount = amount,
                BaseCurrency = baseCurrency,
                Date = date,
                Rates = paginatedRates,
                TotalRatesCount = totalItems,
                CurrentPage = page,
                RatesCount = paginatedRates.Count()
            };

            return response;
        }

        public async Task<TimeSeriesResponse> GetTimeSeriesDataAsync(
            DateTime startDate,
            DateTime endDate,
            string baseCurrency = "EUR",
            decimal amount = 1.0m,
            int page = 1,
            int pageSize = 20)
        {
            string cacheKey = $"timeseries_{startDate:yyyy-MM-dd}_{endDate:yyyy-MM-dd}_{baseCurrency}_{amount}_{page}_{pageSize}";

            if (!_cache.TryGetValue(cacheKey, out TimeSeriesResponse cachedData))
            {
                cachedData = await GetAsync<TimeSeriesResponse>($"{startDate:yyyy-MM-dd}..{endDate:yyyy-MM-dd}?from={baseCurrency}&amount={amount}");
                if (cachedData != null)
                {
                    _cache.Set(cacheKey, cachedData, TimeSpan.FromMinutes(10));
                }
            }

            var totalItems = cachedData.Rates.Count;
            var paginatedRates = cachedData.Rates
                .OrderBy(r => r.Key)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToDictionary(r => r.Key, r => r.Value.Where(r => !UnsupportedCurrencies.ExcludedCurrencies.Contains(r.Key.ToUpper())).ToDictionary());

            
            var response = new TimeSeriesResponse
            {
                Amount = amount,
                BaseCurrency = baseCurrency,
                StartDate = startDate,
                EndDate = endDate,
                Rates = paginatedRates,
                TotalRatesCount = totalItems, 
                CurrentPage = page,
                RatesCount = paginatedRates.Count()
            };

            return response;
        }

        public async Task<CurrenciesResponse> GetAvailableCurrenciesAsync()
        {
            string cacheKey = "available_currencies";

            if (!_cache.TryGetValue(cacheKey, out CurrenciesResponse cachedData))
            {
                cachedData = await GetAsync<CurrenciesResponse>("currencies");
                _cache.Set(cacheKey, cachedData, TimeSpan.FromMinutes(10));
            }

            return cachedData;
        }

        public async Task<bool> IsBaseCurrencySupported(string baseCurrency)
        {
            var supportedCurrencies = await GetAvailableCurrenciesAsync();
            if (supportedCurrencies == null || supportedCurrencies.Count == 0)
            {
                return false;
            }

            return supportedCurrencies.ContainsKey(baseCurrency.ToUpper()) &&
                   !UnsupportedCurrencies.ExcludedCurrencies.Contains(baseCurrency.ToUpper());
        }

        private async Task<T> GetAsync<T>(string endpoint)
        {
            HttpResponseMessage response = await _httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, _jsonOptions);
        }
    }
}
