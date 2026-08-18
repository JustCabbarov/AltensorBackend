using AltensorAuthService.Application.Exceptions;
using AltensorAuthService.Application.Interfaces;
using AltensorAuthService.Contract.Events;
using AltensorAuthService.Contract.Tenants;
using AltensorAuthService.Domain.Entities;
using AltensorAuthService.Domain.Enums;
using AltensorAuthService.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace AltensorAuthService.Application.Services
{
    public class TenantManagementService : ITenantManagementService
    {
        private readonly ITenantRepository _tenantRepository;
        private readonly IUserRepository _userRepository;
        private readonly IModuleRepository _moduleRepository;
        private readonly ITenantModuleSubscriptionRepository _subscriptionRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IIntegrationEventPublisher _eventPublisher;
        private readonly ILogger<TenantManagementService> _logger;

        public TenantManagementService(
            ITenantRepository tenantRepository,
            IUserRepository userRepository,
            IModuleRepository moduleRepository,
            ITenantModuleSubscriptionRepository subscriptionRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IIntegrationEventPublisher eventPublisher,
            ILogger<TenantManagementService> logger)
        {
            _tenantRepository = tenantRepository;
            _userRepository = userRepository;
            _moduleRepository = moduleRepository;
            _subscriptionRepository = subscriptionRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _roleManager = roleManager;
            _eventPublisher = eventPublisher;
            _logger = logger;
        }

        public async Task<TenantResponse> CreateTenantWithAdminAsync(CreateTenantRequest request)
        {
            _logger.LogInformation("Yeni Tenant və Admin yaradılması prosesi başladı: Name='{Name}', Slug='{Slug}', AdminEmail='{AdminEmail}'",
                request.Name, request.Slug, request.AdminEmail);

            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Slug) || string.IsNullOrWhiteSpace(request.AdminEmail) || string.IsNullOrWhiteSpace(request.AdminPassword))
            {
                _logger.LogWarning("Tenant yaradılması uğursuz oldu: Tələb olunan sahələr boşdur.");
                throw new ValidationException("Tenant adı, slug, admin email və şifrə sahələri tələb olunur.");
            }

            var slugExists = await _tenantRepository.SlugExistsAsync(request.Slug);
            if (slugExists)
            {
                _logger.LogWarning("Tenant yaradılması uğursuz oldu: '{Slug}' slug artıq mövcuddur.", request.Slug);
                throw new ValidationException($"'{request.Slug}' slug adı artıq başqa bir müştəri tərəfindən istifadə olunur.");
            }

            // 1. Create Tenant
            var tenant = new Tenant
            {
                Name = request.Name.Trim(),
                Slug = request.Slug.Trim().ToLower(),
                Domain = request.Domain?.Trim().ToLower(),
                Status = TenantStatus.Trial
            };

            await _tenantRepository.AddAsync(tenant);
            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Tenant uğurla yaradıldı: TenantId={TenantId}, Slug='{Slug}'", tenant.Id, tenant.Slug);

            // 2. Add Module Subscriptions
            if (request.ModuleIds != null && request.ModuleIds.Any())
            {
                var modules = await _moduleRepository.GetByIdsAsync(request.ModuleIds);
                foreach (var module in modules)
                {
                    var subscription = new TenantModuleSubscription
                    {
                        TenantId = tenant.Id,
                        ModuleId = module.Id,
                        Status = SubscriptionStatus.Active,
                        StartsAt = DateTime.UtcNow
                    };
                    await _subscriptionRepository.AddAsync(subscription);
                }
                _logger.LogInformation("Tenanta {Count} modul abunəliyi təyin edildi: TenantId={TenantId}", modules.Count, tenant.Id);
            }

            // 3. Ensure TenantAdmin role exists
            var tenantAdminRoleName = "TenantAdmin";
            var roleExists = await _roleManager.RoleExistsAsync(tenantAdminRoleName);
            if (!roleExists)
            {
                _logger.LogInformation("'{RoleName}' rolu tapılmadı, sistem rolu kimi yaradılır.", tenantAdminRoleName);
                var role = new ApplicationRole
                {
                    Name = tenantAdminRoleName,
                    NormalizedName = tenantAdminRoleName.ToUpper(),
                    IsSystemRole = true,
                    Description = "Tenant administrator role with full management permissions inside tenant."
                };
                await _roleManager.CreateAsync(role);
            }

            // 4. Create Initial Tenant Admin User
            var adminUser = new ApplicationUser
            {
                UserName = request.AdminEmail.Trim(),
                Email = request.AdminEmail.Trim(),
                FullName = request.AdminFullName,
                TenantId = tenant.Id,
                IsActive = true
            };

            var userResult = await _userManager.CreateAsync(adminUser, request.AdminPassword);
            if (!userResult.Succeeded)
            {
                var errors = string.Join("; ", userResult.Errors.Select(e => e.Description));
                _logger.LogError("Tenant Admin istifadəçi yaradılarkən xəta: {Errors}", errors);
                throw new ValidationException($"Admin istifadəçi yaradılarkən xəta baş verdi: {errors}");
            }

            await _userManager.AddToRoleAsync(adminUser, tenantAdminRoleName);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Tenant və Admin istifadəçi uğurla tamamlandı: TenantId={TenantId}, AdminUserId={AdminUserId}, AdminEmail='{AdminEmail}'",
                tenant.Id, adminUser.Id, adminUser.Email);

            // Digər modullara yeni admin user haqqında xəbər ver
            await _eventPublisher.PublishUserCreatedAsync(new UserCreatedIntegrationEvent
            {
                UserId = adminUser.Id,
                TenantId = tenant.Id,
                Email = adminUser.Email!,
                FullName = adminUser.FullName,
                UserName = adminUser.UserName,
                CreatedAt = adminUser.CreatedAt
            });

            return new TenantResponse
            {
                Id = tenant.Id,
                Name = tenant.Name,
                Slug = tenant.Slug,
                Domain = tenant.Domain,
                Status = tenant.Status.ToString(),
                CreatedAt = tenant.CreatedAt,
                SuspendedAt = tenant.SuspendedAt
            };
        }

        public async Task<List<TenantResponse>> GetAllTenantsAsync(TenantStatus? status = null)
        {
            _logger.LogInformation("Bütün tenantlar sorğulanır: StatusFilter='{Status}'", status?.ToString() ?? "Hamısı");

            var tenants = status.HasValue
                ? await _tenantRepository.GetByStatusAsync(status.Value)
                : await _tenantRepository.GetAllAsync();

            _logger.LogInformation("Ümumi {Count} tenant tapıldı.", tenants.Count);

            return tenants.Select(t => new TenantResponse
            {
                Id = t.Id,
                Name = t.Name,
                Slug = t.Slug,
                Domain = t.Domain,
                Status = t.Status.ToString(),
                CreatedAt = t.CreatedAt,
                SuspendedAt = t.SuspendedAt
            }).ToList();
        }

        public async Task<TenantDetailResponse> GetTenantDetailAsync(Guid tenantId)
        {
            _logger.LogInformation("Tenant detalları sorğulanır: TenantId={TenantId}", tenantId);

            var tenant = await _tenantRepository.GetByIdAsync(tenantId);
            if (tenant == null)
            {
                _logger.LogWarning("Tenant tapılmadı: TenantId={TenantId}", tenantId);
                throw new NotFoundException($"ID '{tenantId}' olan tenant tapılmadı.");
            }

            var subscriptions = await _subscriptionRepository.GetAllByTenantAsync(tenantId);
            var users = await _userRepository.GetUsersByTenantAsync(tenantId);

            _logger.LogInformation("Tenant detalları gətirildi: TenantId={TenantId}, UsersCount={UsersCount}, SubscriptionsCount={SubsCount}",
                tenantId, users.Count, subscriptions.Count);

            return new TenantDetailResponse
            {
                Id = tenant.Id,
                Name = tenant.Name,
                Slug = tenant.Slug,
                Domain = tenant.Domain,
                Status = tenant.Status.ToString(),
                CreatedAt = tenant.CreatedAt,
                SuspendedAt = tenant.SuspendedAt,
                UserCount = users.Count,
                Subscriptions = subscriptions.Select(s => new TenantModuleSubscriptionDto
                {
                    ModuleId = s.ModuleId,
                    ModuleCode = s.Module?.Code ?? string.Empty,
                    ModuleName = s.Module?.Name ?? string.Empty,
                    Status = s.Status.ToString(),
                    StartsAt = s.StartsAt,
                    ExpiresAt = s.ExpiresAt,
                    SuspendedAt = s.SuspendedAt,
                    SuspendReason = s.SuspendReason
                }).ToList()
            };
        }

        public async Task SuspendTenantAsync(Guid tenantId, string? reason)
        {
            _logger.LogWarning("Tenant dondurulur (Suspend): TenantId={TenantId}, Reason='{Reason}'", tenantId, reason ?? "Göstərilməyib");

            var tenant = await _tenantRepository.GetByIdAsync(tenantId);
            if (tenant == null)
            {
                _logger.LogWarning("Dondurulacaq tenant tapılmadı: TenantId={TenantId}", tenantId);
                throw new NotFoundException($"ID '{tenantId}' olan tenant tapılmadı.");
            }

            if (tenant.Slug.Equals("platform", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Platform tenant dondurula bilməz: TenantId={TenantId}", tenantId);
                throw new ValidationException("Əsas 'platform' tenantı sistemin işləməsi üçün qorunur və dondurula bilməz.");
            }

            tenant.Status = TenantStatus.Suspended;
            tenant.SuspendedAt = DateTime.UtcNow;
            await _tenantRepository.UpdateAsync(tenant);

            // Suspend all modules
            await _subscriptionRepository.SuspendAllModulesAsync(tenantId, reason ?? "Manual suspension by Super Admin (unpaid/billing)");

            // Revoke all refresh tokens for tenant users so they cannot refresh tokens
            await _refreshTokenRepository.RevokeAllForTenantAsync(tenantId);

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Tenant, modulları və bütün istifadəçi sessiyaları uğurla donduruldu: TenantId={TenantId}", tenantId);
        }

        public async Task UnsuspendTenantAsync(Guid tenantId)
        {
            _logger.LogInformation("Tenant bərpa edilir (Unsuspend): TenantId={TenantId}", tenantId);

            var tenant = await _tenantRepository.GetByIdAsync(tenantId);
            if (tenant == null)
            {
                _logger.LogWarning("Bərpa ediləcək tenant tapılmadı: TenantId={TenantId}", tenantId);
                throw new NotFoundException($"ID '{tenantId}' olan tenant tapılmadı.");
            }

            tenant.Status = TenantStatus.Active;
            tenant.SuspendedAt = null;
            await _tenantRepository.UpdateAsync(tenant);

            // Reactivate all modules
            await _subscriptionRepository.ActivateAllModulesAsync(tenantId);

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Tenant və modulları uğurla aktivləşdirildi: TenantId={TenantId}", tenantId);
        }

        public async Task AddModuleSubscriptionAsync(Guid tenantId, ModuleSubscriptionRequest request)
        {
            _logger.LogInformation("Modul abunəliyi əlavə edilir: TenantId={TenantId}, ModuleId={ModuleId}", tenantId, request.ModuleId);

            var tenant = await _tenantRepository.GetByIdAsync(tenantId);
            if (tenant == null)
            {
                _logger.LogWarning("Tenant tapılmadı: TenantId={TenantId}", tenantId);
                throw new NotFoundException($"ID '{tenantId}' olan tenant tapılmadı.");
            }

            var module = await _moduleRepository.GetByIdAsync(request.ModuleId);
            if (module == null)
            {
                _logger.LogWarning("Modul tapılmadı: ModuleId={ModuleId}", request.ModuleId);
                throw new NotFoundException($"ID '{request.ModuleId}' olan modul tapılmadı.");
            }

            var existing = await _subscriptionRepository.GetSubscriptionAsync(tenantId, request.ModuleId);
            if (existing != null)
            {
                _logger.LogInformation("Mövcud modul abunəliyi yenilənir: TenantId={TenantId}, ModuleId={ModuleId}", tenantId, request.ModuleId);
                existing.Status = SubscriptionStatus.Active;
                existing.ExpiresAt = request.ExpiresAt;
                existing.SuspendedAt = null;
                existing.SuspendReason = null;
                await _subscriptionRepository.UpdateAsync(existing);
            }
            else
            {
                _logger.LogInformation("Yeni modul abunəliyi yaradılır: TenantId={TenantId}, ModuleId={ModuleId}", tenantId, request.ModuleId);
                var sub = new TenantModuleSubscription
                {
                    TenantId = tenantId,
                    ModuleId = request.ModuleId,
                    Status = SubscriptionStatus.Active,
                    StartsAt = DateTime.UtcNow,
                    ExpiresAt = request.ExpiresAt
                };
                await _subscriptionRepository.AddAsync(sub);
            }

            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Modul abunəliyi əməliyyatı uğurla tamamlandı: TenantId={TenantId}, ModuleId={ModuleId}", tenantId, request.ModuleId);
        }

        public async Task RemoveModuleSubscriptionAsync(Guid tenantId, Guid moduleId)
        {
            _logger.LogInformation("Modul abunəliyi silinir: TenantId={TenantId}, ModuleId={ModuleId}", tenantId, moduleId);
            await _subscriptionRepository.DeleteAsync(tenantId, moduleId);
            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Modul abunəliyi uğurla silindi: TenantId={TenantId}, ModuleId={ModuleId}", tenantId, moduleId);
        }

        public async Task SuspendModuleSubscriptionAsync(Guid tenantId, Guid moduleId, string? reason)
        {
            _logger.LogWarning("Modul abunəliyi dondurulur: TenantId={TenantId}, ModuleId={ModuleId}, Reason='{Reason}'", tenantId, moduleId, reason ?? "Göstərilməyib");
            await _subscriptionRepository.SuspendAsync(tenantId, moduleId, reason ?? "Manual module suspension by Super Admin");
            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Modul abunəliyi uğurla donduruldu: TenantId={TenantId}, ModuleId={ModuleId}", tenantId, moduleId);
        }

        public async Task UnsuspendModuleSubscriptionAsync(Guid tenantId, Guid moduleId)
        {
            _logger.LogInformation("Modul abunəliyi aktivləşdirilir: TenantId={TenantId}, ModuleId={ModuleId}", tenantId, moduleId);
            await _subscriptionRepository.ActivateAsync(tenantId, moduleId);
            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Modul abunəliyi uğurla aktivləşdirildi: TenantId={TenantId}, ModuleId={ModuleId}", tenantId, moduleId);
        }
    }
}
