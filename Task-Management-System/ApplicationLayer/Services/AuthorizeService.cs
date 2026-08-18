using Contract.Services;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace ApplicationLayer.Services
{
    public class AuthorizationService : IAuthorizeService
    {
        private readonly IUserRepository _userRepository;
        private readonly ICurrentTenantService _tenantService;
        private readonly ILogger<AuthorizationService> _logger;

        public AuthorizationService(
            IUserRepository userRepository,
            ICurrentTenantService tenantService,
            ILogger<AuthorizationService> logger)
        {
            _userRepository = userRepository;
            _tenantService = tenantService;
            _logger = logger;
        }

        public async Task<List<AppUser>> GetAllUsersAsync()
        {
            var tenantId = _tenantService.TenantId
                ?? throw new UnauthorizedAccessException("Tenant konteksti tapılmadı.");

            _logger.LogInformation("GetAllUsers: TenantId={TenantId}", tenantId);
            return await _userRepository.GetAllUsersAsync(tenantId);
        }

        public async Task<AppUser?> GetUserByIdAsync(Guid userId)
        {
            return await _userRepository.GetByIdAsync(userId);
        }
    }
}
