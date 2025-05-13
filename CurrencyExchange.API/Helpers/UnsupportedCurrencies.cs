namespace CurrencyExchange.API.Helpers
{
    public static class UnsupportedCurrencies
    {
        private static HashSet<string> _unsupportedCurrencies = new HashSet<string> { "TRY", "PLN", "THB", "MXN" };
        public static HashSet<string> ExcludedCurrencies
        {
            get
            {
                return _unsupportedCurrencies;
            }
        }
    }
}
