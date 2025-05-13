using CurrencyExchange.API.Providers;
using CurrencyExchange.API.Providers.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CurrencyExchange.Unit.Tests
{
    public class CurrencyConverterProviderFactoryTests
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CurrencyConverterProviderFactory _factory;

        public CurrencyConverterProviderFactoryTests()
        {
            _serviceProvider = TestSetup.CreateServiceProvider();
            _factory = new CurrencyConverterProviderFactory(_serviceProvider);
        }

        [Fact]
        public void GetProvider_ShouldReturnFrankfurterProvider_WhenProviderNameIsNullOrEmpty()
        {
            var provider = _factory.GetProvider("");
            Assert.IsType<FrankfurterProvider>(provider);

            provider = _factory.GetProvider(null);
            Assert.IsType<FrankfurterProvider>(provider);

            provider = _factory.GetProvider("Frankfurter");
            Assert.IsType<FrankfurterProvider>(provider);
        }

        [Fact]
        public void GetProvider_ShouldThrowArgumentException_WhenProviderIsNotSupported()
        {
            Assert.Throws<ArgumentException>(() => _factory.GetProvider("NonExistentProvider"));
        }
    }
}