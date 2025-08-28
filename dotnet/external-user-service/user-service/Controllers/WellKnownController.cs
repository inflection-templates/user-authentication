using Microsoft.AspNetCore.Mvc;
using AspNetJwtAuth.Services.Interfaces;

namespace AspNetJwtAuth.Controllers
{
    [ApiController]
    [Route(".well-known")]
    public class WellKnownController : ControllerBase
    {
        private readonly IJwtTokenService _jwtTokenService;
        private readonly ILogger<WellKnownController> _logger;

        public WellKnownController(
            IJwtTokenService jwtTokenService,
            ILogger<WellKnownController> logger)
        {
            _jwtTokenService = jwtTokenService;
            _logger = logger;
        }

        /// <summary>
        /// JWKS endpoint for public key retrieval by other services
        /// RFC 7517: https://tools.ietf.org/html/rfc7517
        /// </summary>
        [HttpGet("jwks.json")]
        public IActionResult GetJwks()
        {
            try
            {
                var jwks = _jwtTokenService.GenerateJwks();
                _logger.LogInformation("JWKS endpoint accessed by external service at standard path");
                return Content(jwks, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error serving JWKS");
                return StatusCode(500);
            }
        }
    }
}
