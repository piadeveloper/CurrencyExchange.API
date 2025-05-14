using System.Text.Json.Serialization;

namespace CurrencyExchange.API.Models
{
    public class TimeSeriesResponse: PageBasedResponse
    {
        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("base")]
        public required string BaseCurrency { get; set; }

        [JsonPropertyName("start_date")]
        public DateTime StartDate { get; set; }

        [JsonPropertyName("end_date")]
        public DateTime EndDate { get; set; }

        [JsonPropertyName("rates")]
        public Dictionary<DateTime, Dictionary<string, decimal>>? Rates { get; set; }
    }
}
