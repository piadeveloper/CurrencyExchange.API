using CurrencyExchange.API.Models;
using CurrencyExchange.API.Providers.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyExchange.API.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Authorize(Roles = "User")]
    [UnsupportedCurrencyFilter]
    public class CurrencyConverterController : ControllerBase
    {
        private readonly ILogger<CurrencyConverterController> _logger;
        private readonly ICurrencyConverterProviderFactory _providerFactory;

        public CurrencyConverterController(
            ILogger<CurrencyConverterController> logger,
            ICurrencyConverterProviderFactory providerFactory)
        {
            _logger = logger;
            _providerFactory = providerFactory;
        }

        /// <summary>
        /// Get latest currency rates.
        /// </summary>
        /// <param name="baseCurrency">Base currency code (default: EUR).</param>
        /// <param name="amount">Amount for convertation (default: 1).</param>
        [HttpGet("latest")]
        public async Task<ActionResult<CurrencyRatesResponse>> GetLatestRates(
            [FromQuery] string provider = "Frankfurter",
            [FromQuery] string baseCurrency = "EUR",
            [FromQuery] decimal amount = 1.0m)
        {
            if (amount <= 0)
                return BadRequest("Amount should be greater than zero");

            var providerInstance = _providerFactory.GetProvider(provider);

            if (!await providerInstance.IsBaseCurrencySupported(baseCurrency))
                return BadRequest($"Currency with code {baseCurrency} is not supporting");

            var result = await providerInstance.GetLatestRatesAsync(baseCurrency, amount);
            return Ok(result);
        }

        /// <summary>
        /// Get historical currency rates by date.
        /// </summary>
        /// <param name="date">Date in format yyyy-MM-dd.</param>
        /// <param name="baseCurrency">Base currency code (default: EUR).</param>
        /// <param name="amount">Currency amount (default: 1).</param>
        /// <param name="page">Number of page for getting rates</param>
        /// <param name="pageSize">Count of currency rates per page</param>
        [HttpGet("historical/{date}")]
        public async Task<ActionResult<HistoricalRatesResponse>> GetHistoricalRates(
            DateTime date,
            [FromQuery] string provider = "Frankfurter",
            [FromQuery] string baseCurrency = "EUR",
            [FromQuery] decimal amount = 1.0m,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            if (amount <= 0)
                return BadRequest("Amount should be greater than zero");

            if (date > DateTime.Now)
                return BadRequest("Date should be in the past");

            var providerInstance = _providerFactory.GetProvider(provider);

            if (!await providerInstance.IsBaseCurrencySupported(baseCurrency))
                return BadRequest($"Currency with code {baseCurrency} is not supporting");

            var result = await providerInstance.GetHistoricalRatesAsync(date, baseCurrency, amount, page, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// Get currency rates by period (time series).
        /// </summary>
        /// <param name="startDate">Start period date in format yyyy-MM-dd.</param>
        /// <param name="endDate">End period date in format yyyy-MM-dd.</param>
        /// <param name="baseCurrency">Base currency code (default: EUR).</param>
        /// <param name="amount">Currency amount (default: 1).</param>
        /// <param name="page">Number of page for getting rates</param>
        /// <param name="pageSize">Count of currency rates per page</param>
        [HttpGet("timeseries/{startDate}/{endDate}")]
        public async Task<ActionResult<TimeSeriesResponse>> GetTimeSeries(
            DateTime startDate,
            DateTime endDate,
            [FromQuery] string provider = "Frankfurter",
            [FromQuery] string baseCurrency = "EUR",
            [FromQuery] decimal amount = 1.0m,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            if (amount <= 0)
                return BadRequest("Amount should be greater than zero");

            if (page < 1 || pageSize < 1)
                return BadRequest("Page and pageSize must be greater than zero.");

            if (startDate > endDate)
                return BadRequest("Start date must be less than or equal to end date.");

            var providerInstance = _providerFactory.GetProvider(provider);

            if (!await providerInstance.IsBaseCurrencySupported(baseCurrency))
                return BadRequest($"Currency with code {baseCurrency} is not supporting");

            var result = await providerInstance.GetTimeSeriesDataAsync(startDate, endDate, baseCurrency, amount, page, pageSize);

            return Ok(result);
        }

        /// <summary>
        /// Get available currencies.
        /// </summary>
        [HttpGet("currencies")]
        public async Task<ActionResult<CurrenciesResponse>> GetAvailableCurrencies(
            [FromQuery] string provider = "Frankfurter")
        {
            var providerInstance = _providerFactory.GetProvider(provider);
            var result = await providerInstance.GetAvailableCurrenciesAsync();
            return Ok(result);
        }
    }
}
