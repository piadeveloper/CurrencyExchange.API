using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace CurrencyExchange.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VersionController : ControllerBase
    {
        /// <summary>
        /// Returns the version of the application.
        /// </summary>
        [HttpGet("getversion")]
        public IActionResult GetVersion()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";
            return Ok(new { Version = version });
        }
    }
}
