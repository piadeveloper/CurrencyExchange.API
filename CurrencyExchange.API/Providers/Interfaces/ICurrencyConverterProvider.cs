using CurrencyExchange.API.Models;
using System.Threading.Tasks;

namespace CurrencyExchange.API.Providers.Interfaces
{
    public interface ICurrencyConverterProvider
    {
        Task<CurrencyRatesResponse> GetLatestRatesAsync(string baseCurrency = "EUR", decimal amount = 1.0m);

        Task<HistoricalRatesResponse> GetHistoricalRatesAsync(DateTime date, string baseCurrency = "EUR", decimal amount = 1.0m, int page = 1, int pageSize = 20);

        Task<TimeSeriesResponse> GetTimeSeriesDataAsync(DateTime startDate, DateTime endDate, string baseCurrency = "EUR", decimal amount = 1.0m, int page = 1, int pageSize = 20);

        Task<CurrenciesResponse> GetAvailableCurrenciesAsync();

        Task<bool> IsBaseCurrencySupported(string baseCurrency);
    }
}
