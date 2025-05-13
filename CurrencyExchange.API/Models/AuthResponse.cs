namespace CurrencyExchange.API.Models
{
    public class AuthResponse
    {
        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
