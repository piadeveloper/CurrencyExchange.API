namespace CurrencyExchange.API.Models
{
    public class AuthResponse
    {
        public required string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
