using AltensorAuthService.Application.Interfaces;
using AltensorAuthService.Contract.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AltensorAuthService.Presentation.Controllers
{
    [ApiController]
    [Route(".well-known")]
    public class WellKnownController : ControllerBase
    {
        private readonly IJwtTokenService _jwtTokenService;

        public WellKnownController(IJwtTokenService jwtTokenService)
        {
            _jwtTokenService = jwtTokenService;
        }

        /// <summary>
        /// Bütün mikroservislər (CRM, Inventory və s.) üçün açıq RSA Public Key (JWKS formatı)
        /// </summary>
        [AllowAnonymous]
        [HttpGet("jwks.json")]
        [ProducesResponseType(typeof(JwksDto), StatusCodes.Status200OK)]
        public IActionResult GetJwks()
        {
            var jwks = _jwtTokenService.GetJwks();
            return Ok(jwks);
        }
    }
}
