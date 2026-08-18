using AltensorAuthService.Application.Interfaces;
using AltensorAuthService.Contract.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AltensorAuthService.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class PermissionsController : ControllerBase
    {
        private readonly IRoleManagementService _roleManagementService;

        public PermissionsController(IRoleManagementService roleManagementService)
        {
            _roleManagementService = roleManagementService;
        }

        /// <summary>
        /// Sistemdə mövcud olan sabit icazə kataloqu (rol yaradılarkən frontend-də seçim üçün)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<PermissionResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllPermissions([FromQuery] string? module = null)
        {
            var permissions = await _roleManagementService.GetAllPermissionsAsync(module);
            return Ok(permissions);
        }
    }
}
