using AltensorAuthService.Application.Interfaces;
using AltensorAuthService.Contract.Roles;
using AltensorAuthService.Contract.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AltensorAuthService.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "TenantAdmin,PlatformSuperAdmin")]
    public class TenantController : ControllerBase
    {
        private readonly IUserManagementService _userService;
        private readonly IRoleManagementService _roleService;

        public TenantController(
            IUserManagementService userService,
            IRoleManagementService roleService)
        {
            _userService = userService;
            _roleService = roleService;
        }

        // ==================== USER MANAGEMENT ====================

        /// <summary>
        /// Cari müştərinin daxilində yeni işçi (istifadəçi) yaratmaq
        /// </summary>
        [HttpPost("users")]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            var user = await _userService.CreateUserAsync(request);
            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
        }

        /// <summary>
        /// Cari müştərinin bütün istifadəçilərinin siyahısı
        /// </summary>
        [HttpGet("users")]
        [ProducesResponseType(typeof(List<UserResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userService.GetTenantUsersAsync();
            return Ok(users);
        }

        /// <summary>
        /// Cari müştərinin tək bir istifadəçisinin detalları
        /// </summary>
        [HttpGet("users/{id:guid}")]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserById([FromRoute] Guid id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            return Ok(user);
        }

        /// <summary>
        /// İstifadəçinin məlumatlarını yeniləmək
        /// </summary>
        [HttpPut("users/{id:guid}")]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateUser([FromRoute] Guid id, [FromBody] UpdateUserRequest request)
        {
            var updated = await _userService.UpdateUserAsync(id, request);
            return Ok(updated);
        }

        /// <summary>
        /// İstifadəçini deaktiv etmək
        /// </summary>
        [HttpPost("users/{id:guid}/deactivate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DeactivateUser([FromRoute] Guid id)
        {
            await _userService.DeactivateUserAsync(id);
            return Ok(new { message = "İstifadəçi hesabı deaktiv edildi." });
        }

        /// <summary>
        /// Deaktiv edilmiş istifadəçini yenidən aktivləşdirmək
        /// </summary>
        [HttpPost("users/{id:guid}/activate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ActivateUser([FromRoute] Guid id)
        {
            await _userService.ActivateUserAsync(id);
            return Ok(new { message = "İstifadəçi hesabı aktivləşdirildi." });
        }

        /// <summary>
        /// İstifadəçiyə rol təyin etmək
        /// </summary>
        [HttpPost("users/{id:guid}/roles")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> AssignRole([FromRoute] Guid id, [FromBody] AssignRoleRequest request)
        {
            await _userService.AssignRoleAsync(id, request.RoleId);
            return Ok(new { message = "Rol istifadəçiyə uğurla təyin edildi." });
        }

        /// <summary>
        /// İstifadəçidən rolu çıxarmaq
        /// </summary>
        [HttpDelete("users/{id:guid}/roles/{roleId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> RemoveRole([FromRoute] Guid id, [FromRoute] Guid roleId)
        {
            await _userService.RemoveRoleAsync(id, roleId);
            return Ok(new { message = "Rol istifadəçidən çıxarıldı." });
        }

        // ==================== ROLE MANAGEMENT ====================

        /// <summary>
        /// Cari müştərinin daxilində mövcud icazələrdən seçərək yeni xüsusi rol yaratmaq
        /// </summary>
        [HttpPost("roles")]
        [ProducesResponseType(typeof(RoleResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
        {
            var role = await _roleService.CreateRoleAsync(request);
            return CreatedAtAction(nameof(GetRoleById), new { id = role.Id }, role);
        }

        /// <summary>
        /// Cari müştərinin istifadə edə biləcəyi bütün rolların siyahısı (sistem + xüsusi)
        /// </summary>
        [HttpGet("roles")]
        [ProducesResponseType(typeof(List<RoleResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _roleService.GetVisibleRolesAsync();
            return Ok(roles);
        }

        /// <summary>
        /// Tək bir rolun detalları və icazələri
        /// </summary>
        [HttpGet("roles/{id:guid}")]
        [ProducesResponseType(typeof(RoleResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRoleById([FromRoute] Guid id)
        {
            var role = await _roleService.GetRoleByIdAsync(id);
            return Ok(role);
        }

        /// <summary>
        /// Xüsusi rolu və onun icazələrini redaktə etmək (sistem rolları redaktə edilə bilməz)
        /// </summary>
        [HttpPut("roles/{id:guid}")]
        [ProducesResponseType(typeof(RoleResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateRole([FromRoute] Guid id, [FromBody] UpdateRoleRequest request)
        {
            var role = await _roleService.UpdateRoleAsync(id, request);
            return Ok(role);
        }

        /// <summary>
        /// Xüsusi rolu silmək (sistem rolları silinə bilməz)
        /// </summary>
        [HttpDelete("roles/{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteRole([FromRoute] Guid id)
        {
            await _roleService.DeleteRoleAsync(id);
            return Ok(new { message = "Rol uğurla silindi." });
        }
    }
}
