namespace CurrencyExchange.API.Models
{
    public class PageBasedResponse
    {
        public int TotalRatesCount { get; set; }

        public int CurrentPage { get; set; }

        public int RatesCount { get; set; }
    }
}
