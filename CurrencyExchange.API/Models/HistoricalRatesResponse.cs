using System.Text.Json.Serialization;

namespace CurrencyExchange.API.Models
{
    public class HistoricalRatesResponse: PageBasedResponse
    {
        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("base")]
        public string BaseCurrency { get; set; }

        [JsonPropertyName("date")]
        public DateTime Date { get; set; }

        [JsonPropertyName("rates")]
        public Dictionary<string, decimal> Rates { get; set; }
    }
}
