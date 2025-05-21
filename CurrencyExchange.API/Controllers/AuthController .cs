using CurrencyExchange.API.Models;
using CurrencyExchange.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyExchange.API.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class AuthController : ControllerBase
    {
        private readonly JwtTokenService _jwtTokenService;

        public AuthController(JwtTokenService jwtTokenService)
        {
            _jwtTokenService = jwtTokenService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromQuery] string username, [FromQuery] string UserSecret)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(UserSecret))
                return BadRequest("Username and UserSecret is required.");

            // TO DO: implement real user name and password checks
            if (username == "invalid_key")
                return Unauthorized("User name is invalid");
            

            var token = _jwtTokenService.GenerateToken(username);

            return Ok(new AuthResponse
            {
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60)
            });
        }
    }
}
