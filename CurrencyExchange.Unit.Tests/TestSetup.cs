using Microsoft.Extensions.DependencyInjection;
using CurrencyExchange.API.Providers;
using CurrencyExchange.API.Providers.Interfaces;

namespace CurrencyExchange.Unit.Tests
{
    public class TestSetup
    {
        public static IServiceProvider CreateServiceProvider()
        {
            var services = new ServiceCollection();

            services.AddHttpClient<FrankfurterProvider>();

            services.AddTransient<ICurrencyConverterProvider, FrankfurterProvider>();

            services.AddTransient<ICurrencyConverterProviderFactory, CurrencyConverterProviderFactory>();

            services.AddMemoryCache();

            return services.BuildServiceProvider();
        }
    }
}