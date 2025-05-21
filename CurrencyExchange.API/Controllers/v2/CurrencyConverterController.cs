using CurrencyExchange.API.Models;
using CurrencyExchange.API.Providers.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyExchange.API.Controllers.V2
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("2.0")]
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

        [HttpPost("latest")]
        public async Task<ActionResult<CurrencyRatesResponse>> GetLatestRates([FromBody] LatestRatesRequest request)
        {
            var provider = _providerFactory.GetProvider(request.Provider);

            if (!await provider.IsBaseCurrencySupported(request.BaseCurrency))
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Unsupported currency",
                    Detail = $"Currency '{request.BaseCurrency}' is not supported by provider '{request.Provider}'.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var result = await provider.GetLatestRatesAsync(request.BaseCurrency, request.Amount);
            return Ok(result);
        }

        [HttpPost("historical/{date:datetime}")]
        public async Task<ActionResult<HistoricalRatesResponse>> GetHistoricalRates(
            DateTime date,
            [FromBody] HistoricalRatesRequest request)
        {
            if (date > DateTime.UtcNow)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid date",
                    Detail = "Date must be in the past.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var provider = _providerFactory.GetProvider(request.Provider);

            if (!await provider.IsBaseCurrencySupported(request.BaseCurrency))
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Unsupported currency",
                    Detail = $"Currency '{request.BaseCurrency}' is not supported.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var result = await provider.GetHistoricalRatesAsync(date, request.BaseCurrency, request.Amount, request.Page, request.PageSize);
            return Ok(result);
        }

        [HttpPost("timeseries")]
        public async Task<ActionResult<TimeSeriesResponse>> GetTimeSeries([FromBody] TimeSeriesRequest request)
        {
            if (request.StartDate > request.EndDate)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid date range",
                    Detail = "Start date must be less than or equal to end date.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var provider = _providerFactory.GetProvider(request.Provider);

            if (!await provider.IsBaseCurrencySupported(request.BaseCurrency))
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Unsupported currency",
                    Detail = $"Currency '{request.BaseCurrency}' is not supported.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var result = await provider.GetTimeSeriesDataAsync(
                request.StartDate, request.EndDate,
                request.BaseCurrency, request.Amount,
                request.Page, request.PageSize);

            return Ok(result);
        }

        [HttpPost("currencies")]
        public async Task<ActionResult<CurrenciesResponse>> GetAvailableCurrencies([FromBody] CurrenciesRequest request)
        {
            var provider = _providerFactory.GetProvider(request.Provider);
            var result = await provider.GetAvailableCurrenciesAsync();
            return Ok(result);
        }
    }
}