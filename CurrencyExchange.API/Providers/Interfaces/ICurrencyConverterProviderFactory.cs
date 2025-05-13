namespace CurrencyExchange.API.Providers.Interfaces
{
    public interface ICurrencyConverterProviderFactory
    {
        ICurrencyConverterProvider GetProvider(string providerName);
    }
}
