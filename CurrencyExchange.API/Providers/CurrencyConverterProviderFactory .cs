using CurrencyExchange.API.Providers.Interfaces;

namespace CurrencyExchange.API.Providers
{
    public class CurrencyConverterProviderFactory: ICurrencyConverterProviderFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public CurrencyConverterProviderFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public ICurrencyConverterProvider GetProvider(string providerName)
        {
            switch (providerName)
            {
                case null:
                case "":
                case "Frankfurter":
                    return _serviceProvider.GetRequiredService<FrankfurterProvider>();
                default:
                    throw new ArgumentException($"Provider {providerName} is not supported.");
            }
        }
    }
}
