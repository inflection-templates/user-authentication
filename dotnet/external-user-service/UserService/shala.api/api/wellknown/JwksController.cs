using Microsoft.AspNetCore.Mvc;
using shala.api.services;

namespace shala.api;

/// <summary>
/// JWKS (JSON Web Key Set) controller for exposing public keys
/// This allows other services to validate JWT tokens issued by this service
/// </summary>
[ApiController]
[Route(".well-known")]
public class JwksController : ControllerBase
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<JwksController> _logger;

    public JwksController(
        IJwtTokenService jwtTokenService,
        ILogger<JwksController> logger)
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
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Health check endpoint for JWKS service
    /// </summary>
    [HttpGet("jwks/health")]
    public IActionResult GetJwksHealth()
    {
        try
        {
            var signingKey = _jwtTokenService.GetSigningKey();
            return Ok(new
            {
                Status = "Healthy",
                KeyType = signingKey.GetType().Name,
                KeyId = signingKey.KeyId,
                Algorithm = "RS256",
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking JWKS health");
            return StatusCode(500, new { Status = "Unhealthy", Error = ex.Message });
        }
    }
}
