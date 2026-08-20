using Contract.Services;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApplicationLayer.Services
{
    public class AuthorizationService : IAuthorizeService
    {
        private readonly IUserRepository _userRepository;
        private readonly IAppUserRepository _appUserRepository;
        private readonly ICurrentTenantService _tenantService;
        private readonly ILogger<AuthorizationService> _logger;

        public AuthorizationService(
            IUserRepository userRepository,
            IAppUserRepository appUserRepository,
            ICurrentTenantService tenantService,
            ILogger<AuthorizationService> logger)
        {
            _userRepository = userRepository;
            _appUserRepository = appUserRepository;
            _tenantService = tenantService;
            _logger = logger;
        }

        public async Task<List<AppUser>> GetAllUsersAsync()
        {
            var tenantId = _tenantService.TenantId
                ?? throw new UnauthorizedAccessException("Tenant konteksti tapılmadı.");

            // Daxil olmuş cari istifadəçini bazada yarad/yenilə
            if (_tenantService.UserId.HasValue && _tenantService.UserId.Value != Guid.Empty)
            {
                try
                {
                    await _appUserRepository.EnsureExistsAsync(
                        _tenantService.UserId.Value,
                        tenantId,
                        _tenantService.Email ?? "user@system.local",
                        _tenantService.Email,
                        _tenantService.Email
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Cari istifadəçi sinxronlaşdırılarkən xəbərdarlıq.");
                }
            }

            _logger.LogInformation("GetAllUsers: TenantId={TenantId}", tenantId);
            return await _userRepository.GetAllUsersAsync(tenantId);
        }

        public async Task<AppUser?> GetUserByIdAsync(Guid userId)
        {
            return await _userRepository.GetByIdAsync(userId);
        }
    }
}
