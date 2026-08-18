using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Altensorcrm.Contract.DTOs.UserManagement;
using Altensorcrm.Contract.Extensions;
using Altensorcrm.Contract.Services.Tenant;
using Altensorcrm.Contract.Services.UserManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Altensorcrm.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ICurrentTenantService _tenantService;

        public UsersController(IUserService userService, ICurrentTenantService tenantService)
        {
            _userService = userService;
            _tenantService = tenantService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var result = await _userService.GetAllAsync(cancellationToken);
            return Ok(result);
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUserProfile(CancellationToken cancellationToken)
        {
            var userId = _tenantService.UserId ?? User.GetUserId();

            if (userId.HasValue)
            {
                var user = await _userService.GetByIdAsync(userId.Value, cancellationToken);
                if (user != null) return Ok(user);
            }

            var allUsers = await _userService.GetAllAsync(cancellationToken);
            var firstUser = allUsers.FirstOrDefault(u => u.Role == "Admin" || u.Role == "TenantAdmin") ?? allUsers.FirstOrDefault();
            return Ok(firstUser);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var user = await _userService.GetByIdAsync(id, cancellationToken);
            if (user == null) return NotFound(new { message = "User not found" });
            return Ok(user);
        }

        [HttpPut("{id:guid}/profile")]
        public async Task<IActionResult> UpdateProfile(Guid id, [FromBody] UpdateUserProfileDto dto, CancellationToken cancellationToken)
        {
            var result = await _userService.UpdateProfileAsync(id, dto, cancellationToken);
            if (result == null) return NotFound(new { message = "User not found" });
            return Ok(result);
        }

        [HttpPost("{id:guid}/avatar")]
        public async Task<IActionResult> UploadAvatar(Guid id, IFormFile file, CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No file uploaded." });
            }

            var wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadsDir = Path.Combine(wwwrootPath, "uploads", "avatars");
            if (!Directory.Exists(uploadsDir))
            {
                Directory.CreateDirectory(uploadsDir);
            }

            var extension = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(extension)) extension = ".png";
            var fileName = $"{id}_{DateTime.UtcNow.Ticks}{extension}";
            var filePath = Path.Combine(uploadsDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            var avatarUrl = $"/uploads/avatars/{fileName}";
            await _userService.UpdateAvatarAsync(id, avatarUrl, cancellationToken);

            return Ok(new { avatarUrl });
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "TenantAdmin,PlatformSuperAdmin,Admin")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var result = await _userService.DeleteAsync(id, cancellationToken);
            return Ok(new { success = result });
        }

        [HttpGet("sales-hierarchy")]
        public async Task<IActionResult> GetSalesHierarchy(CancellationToken cancellationToken)
        {
            var result = await _userService.GetSalesHierarchyAsync(cancellationToken);
            return Ok(result);
        }
    }
}
