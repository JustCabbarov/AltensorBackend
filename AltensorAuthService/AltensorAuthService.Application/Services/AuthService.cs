using AltensorAuthService.Application.Exceptions;
using AltensorAuthService.Application.Interfaces;
using AltensorAuthService.Contract.Auth;
using AltensorAuthService.Domain.Entities;
using AltensorAuthService.Domain.Enums;
using AltensorAuthService.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace AltensorAuthService.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly ITenantRepository _tenantRepository;
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly ITenantModuleSubscriptionRepository _subscriptionRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly ICurrentTenantService _currentTenantService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            ITenantRepository tenantRepository,
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            ITenantModuleSubscriptionRepository subscriptionRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IUnitOfWork unitOfWork,
            IJwtTokenService jwtTokenService,
            ICurrentTenantService currentTenantService,
            UserManager<ApplicationUser> userManager,
            ILogger<AuthService> logger)
        {
            _tenantRepository = tenantRepository;
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _subscriptionRepository = subscriptionRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _unitOfWork = unitOfWork;
            _jwtTokenService = jwtTokenService;
            _currentTenantService = currentTenantService;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<TokenResponse> LoginAsync(LoginRequest request)
        {
            _logger.LogInformation("Giriş cəhdi başladı: Email='{Email}', TenantSlug='{TenantSlug}'", request.Email, request.TenantSlug);

            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password) || string.IsNullOrWhiteSpace(request.TenantSlug))
            {
                _logger.LogWarning("Giriş uğursuz oldu: Tələb olunan sahələr boşdur.");
                throw new ValidationException("Email, şifrə və tenant slug sahələri tələb olunur.");
            }

            var tenant = await _tenantRepository.GetBySlugAsync(request.TenantSlug);
            if (tenant == null)
            {
                _logger.LogWarning("Giriş uğursuz oldu: Qeyd olunan müştəri tapılmadı (TenantSlug='{TenantSlug}').", request.TenantSlug);
                throw new UnauthorizedException("Qeyd olunan müştəri (tenant) tapılmadı.");
            }

            if (tenant.Status == TenantStatus.Suspended)
            {
                _logger.LogWarning("Giriş rədd edildi: Tenant dondurulub. TenantId={TenantId}, Slug='{Slug}'", tenant.Id, tenant.Slug);
                throw new TenantSuspendedException("Şirkətinizin hesabı dondurulub (ödəniş edilməyib). Platform administratoru ilə əlaqə saxlayın.");
            }

            if (tenant.Status == TenantStatus.Expired)
            {
                _logger.LogWarning("Giriş rədd edildi: Tenant abunəlik müddəti bitib. TenantId={TenantId}, Slug='{Slug}'", tenant.Id, tenant.Slug);
                throw new TenantSuspendedException("Şirkətinizin abunəlik müddəti bitib.");
            }

            var user = await _userRepository.GetByEmailAndTenantAsync(request.Email, tenant.Id);
            if (user == null)
            {
                _logger.LogWarning("Giriş uğursuz oldu: İstifadəçi tapılmadı. Email='{Email}', TenantId={TenantId}", request.Email, tenant.Id);
                throw new UnauthorizedException("Email və ya şifrə yanlışdır.");
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Giriş rədd edildi: İstifadəçi hesabı deaktivdir. UserId={UserId}, Email='{Email}'", user.Id, request.Email);
                throw new UnauthorizedException("İstifadəçi hesabı deaktiv edilib.");
            }

            var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!passwordValid)
            {
                _logger.LogWarning("Giriş uğursuz oldu: Şifrə yanlışdır. UserId={UserId}, Email='{Email}'", user.Id, request.Email);
                throw new UnauthorizedException("Email və ya şifrə yanlışdır.");
            }

            // Get roles and permissions
            var userRoles = await _userManager.GetRolesAsync(user);
            var permissions = new HashSet<string>();

            var visibleRoles = await _roleRepository.GetVisibleRolesForTenantAsync(tenant.Id);
            var matchingRoles = visibleRoles.Where(r => userRoles.Contains(r.Name!)).ToList();

            foreach (var role in matchingRoles)
            {
                if (role.RolePermissions != null)
                {
                    foreach (var rp in role.RolePermissions)
                    {
                        if (rp.Permission != null)
                        {
                            permissions.Add(rp.Permission.Code);
                        }
                    }
                }
            }

            // Get active modules
            var activeModules = await _subscriptionRepository.GetActiveModuleCodesAsync(tenant.Id);

            _logger.LogDebug("İstifadəçi məlumatları toplandı: UserId={UserId}, Rollar=[{Roles}], İcazələr sayı={PermCount}, Aktiv modullar=[{Modules}]",
                user.Id, string.Join(", ", userRoles), permissions.Count, string.Join(", ", activeModules));

            // Generate tokens
            var accessToken = await _jwtTokenService.GenerateAccessTokenAsync(
                user,
                tenant.Status.ToString(),
                userRoles,
                permissions,
                activeModules);

            var rawRefreshToken = _jwtTokenService.GenerateRawRefreshToken();
            var refreshTokenHash = _jwtTokenService.HashRefreshToken(rawRefreshToken);

            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                TokenHash = refreshTokenHash,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                Revoked = false
            };

            await _refreshTokenRepository.AddAsync(refreshTokenEntity);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Giriş uğurla tamamlandı: UserId={UserId}, TenantId={TenantId}, Email='{Email}'", user.Id, tenant.Id, user.Email);

            return new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = rawRefreshToken,
                ExpiresIn = 900 // 15 dəqiqə
            };
        }

        public async Task<TokenResponse> RefreshAsync(RefreshRequest request)
        {
            _logger.LogInformation("Token yeniləmə (Refresh) sorğusu başladı.");

            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                _logger.LogWarning("Refresh sorğusu uğursuz: Refresh token boşdur.");
                throw new ValidationException("Refresh token tələb olunur.");
            }

            var tokenHash = _jwtTokenService.HashRefreshToken(request.RefreshToken);
            var storedToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash);

            if (storedToken == null || storedToken.Revoked || storedToken.ExpiresAt <= DateTime.UtcNow)
            {
                _logger.LogWarning("Refresh token etibarsızdır, ləğv edilib və ya müddəti bitib.");
                throw new UnauthorizedException("Refresh token etibarsızdır və ya müddəti bitib.");
            }

            var user = storedToken.User;
            if (user == null || !user.IsActive)
            {
                _logger.LogWarning("Refresh uğursuz: İstifadəçi tapılmadı və ya aktiv deyil.");
                throw new UnauthorizedException("İstifadəçi hesabı aktiv deyil.");
            }

            // Always re-read tenant status directly from DB
            var tenant = await _tenantRepository.GetByIdAsync(user.TenantId);
            if (tenant == null || tenant.Status == TenantStatus.Suspended || tenant.Status == TenantStatus.Expired)
            {
                _logger.LogWarning("Tenant dondurulduğu üçün refresh rədd edildi: TenantId={TenantId}. Bütün tokenlər ləğv edilir.", user.TenantId);
                // Revoke all tokens for suspended tenant
                await _refreshTokenRepository.RevokeAllForTenantAsync(user.TenantId);
                await _unitOfWork.SaveChangesAsync();
                throw new TenantSuspendedException("Şirkətinizin hesabı dondurulub və ya müddəti bitib.");
            }

            // Re-read user roles and permissions
            var userRoles = await _userManager.GetRolesAsync(user);
            var permissions = new HashSet<string>();

            var visibleRoles = await _roleRepository.GetVisibleRolesForTenantAsync(tenant.Id);
            var matchingRoles = visibleRoles.Where(r => userRoles.Contains(r.Name!)).ToList();

            foreach (var role in matchingRoles)
            {
                if (role.RolePermissions != null)
                {
                    foreach (var rp in role.RolePermissions)
                    {
                        if (rp.Permission != null)
                        {
                            permissions.Add(rp.Permission.Code);
                        }
                    }
                }
            }

            // Re-read active modules
            var activeModules = await _subscriptionRepository.GetActiveModuleCodesAsync(tenant.Id);

            // Generate new token pair
            var newAccessToken = await _jwtTokenService.GenerateAccessTokenAsync(
                user,
                tenant.Status.ToString(),
                userRoles,
                permissions,
                activeModules);

            var newRawRefreshToken = _jwtTokenService.GenerateRawRefreshToken();
            var newRefreshTokenHash = _jwtTokenService.HashRefreshToken(newRawRefreshToken);

            // Token rotation: Revoke old token and create new one
            await _refreshTokenRepository.RevokeAsync(tokenHash, "Rotated to new token");

            var newRefreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                TokenHash = newRefreshTokenHash,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                Revoked = false
            };

            await _refreshTokenRepository.AddAsync(newRefreshTokenEntity);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Token rotasiyası və yenilənməsi uğurla icra olundu: UserId={UserId}, TenantId={TenantId}", user.Id, tenant.Id);

            return new TokenResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRawRefreshToken,
                ExpiresIn = 900
            };
        }

        public async Task LogoutAsync(string? rawRefreshToken)
        {
            _logger.LogInformation("Çıxış (Logout) əməliyyatı başladıldı.");

            if (!string.IsNullOrWhiteSpace(rawRefreshToken))
            {
                var tokenHash = _jwtTokenService.HashRefreshToken(rawRefreshToken);
                await _refreshTokenRepository.RevokeAsync(tokenHash, "User logout");
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Refresh token ləğv edildi (Revoked).");
            }
        }

        public async Task LogoutAllDevicesAsync(Guid userId)
        {
            _logger.LogInformation("İstifadəçinin bütün cihazlarından çıxış (Revoke All) başladıldı: UserId={UserId}", userId);
            await _refreshTokenRepository.RevokeAllForUserAsync(userId);
            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("İstifadəçinin bütün tokenləri uğurla ləğv edildi: UserId={UserId}", userId);
        }

        public async Task<UserInfoDto> GetCurrentUserInfoAsync()
        {
            _logger.LogInformation("Cari istifadəçi məlumatları (UserInfo) sorğulanır.");

            if (!_currentTenantService.IsAuthenticated || !_currentTenantService.UserId.HasValue)
            {
                _logger.LogWarning("Cari istifadəçi autentifikasiyadan keçməyib.");
                throw new UnauthorizedException("İstifadəçi daxil olmayıb.");
            }

            var userId = _currentTenantService.UserId.Value;
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                _logger.LogWarning("Cari istifadəçi bazada tapılmadı: UserId={UserId}", userId);
                throw new NotFoundException("İstifadəçi tapılmadı.");
            }

            var tenant = user.Tenant ?? await _tenantRepository.GetByIdAsync(user.TenantId);
            var roles = await _userManager.GetRolesAsync(user);

            var permissions = new HashSet<string>();
            if (tenant != null)
            {
                var visibleRoles = await _roleRepository.GetVisibleRolesForTenantAsync(tenant.Id);
                var matchingRoles = visibleRoles.Where(r => roles.Contains(r.Name!)).ToList();

                foreach (var role in matchingRoles)
                {
                    if (role.RolePermissions != null)
                    {
                        foreach (var rp in role.RolePermissions)
                        {
                            if (rp.Permission != null)
                            {
                                permissions.Add(rp.Permission.Code);
                            }
                        }
                    }
                }
            }

            var modules = tenant != null 
                ? await _subscriptionRepository.GetActiveModuleCodesAsync(tenant.Id) 
                : new List<string>();

            _logger.LogInformation("Cari istifadəçi məlumatları hazırlandı: UserId={UserId}, Email='{Email}', TenantId={TenantId}, RolesCount={RolesCount}, PermissionsCount={PermsCount}, ModulesCount={ModulesCount}",
                user.Id, user.Email, user.TenantId, roles.Count, permissions.Count, modules.Count);

            return new UserInfoDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                TenantId = user.TenantId,
                TenantName = tenant?.Name ?? string.Empty,
                TenantSlug = tenant?.Slug ?? string.Empty,
                TenantStatus = tenant?.Status.ToString() ?? string.Empty,
                Roles = roles.ToList(),
                Permissions = permissions.ToList(),
                Modules = modules
            };
        }
    }
}
