using AltensorAuthService.Application.Exceptions;
using AltensorAuthService.Application.Interfaces;
using AltensorAuthService.Contract.Events;
using AltensorAuthService.Contract.Users;
using AltensorAuthService.Domain.Entities;
using AltensorAuthService.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace AltensorAuthService.Application.Services
{
    public class UserManagementService : IUserManagementService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly ICurrentTenantService _currentTenantService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IIntegrationEventPublisher _eventPublisher;
        private readonly ILogger<UserManagementService> _logger;

        public UserManagementService(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            ICurrentTenantService currentTenantService,
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IIntegrationEventPublisher eventPublisher,
            ILogger<UserManagementService> logger)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _currentTenantService = currentTenantService;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _roleManager = roleManager;
            _eventPublisher = eventPublisher;
            _logger = logger;
        }

        private Guid GetRequiredTenantId()
        {
            if (!_currentTenantService.TenantId.HasValue)
            {
                _logger.LogWarning("Tenant konteksti tapılmadı.");
                throw new UnauthorizedException("Tenant konteksti tapılmadı.");
            }
            return _currentTenantService.TenantId.Value;
        }

        public async Task<UserResponse> CreateUserAsync(CreateUserRequest request)
        {
            var tenantId = GetRequiredTenantId();
            _logger.LogInformation("İstifadəçi yaradılması başladı: Email='{Email}', TenantId={TenantId}", request.Email, tenantId);

            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                _logger.LogWarning("İstifadəçi yaradılması uğursuz oldu: Email və ya şifrə boşdur.");
                throw new ValidationException("Email və şifrə sahələri tələb olunur.");
            }

            var emailExists = await _userRepository.EmailExistsInTenantAsync(request.Email, tenantId);
            if (emailExists)
            {
                _logger.LogWarning("İstifadəçi artıq mövcuddur: Email='{Email}', TenantId={TenantId}", request.Email, tenantId);
                throw new ValidationException($"'{request.Email}' email ünvanı ilə bu şirkətdə artıq bir istifadəçi mövcuddur.");
            }

            var user = new ApplicationUser
            {
                UserName = request.Email.Trim(),
                Email = request.Email.Trim(),
                FullName = request.FullName?.Trim(),
                TenantId = tenantId,
                IsActive = true,
                CreatedByUserId = _currentTenantService.UserId
            };

            var createResult = await _userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
            {
                var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                _logger.LogError("İstifadəçi Identity yaradılarkən xəta: {Errors}", errors);
                throw new ValidationException($"İstifadəçi yaradılarkən xəta baş verdi: {errors}");
            }

            var assignedRoles = new List<string>();
            if (request.RoleIds != null && request.RoleIds.Any())
            {
                var visibleRoles = await _roleRepository.GetVisibleRolesForTenantAsync(tenantId);
                var validRoles = visibleRoles.Where(r => request.RoleIds.Contains(r.Id)).ToList();

                foreach (var role in validRoles)
                {
                    if (!string.IsNullOrWhiteSpace(role.Name))
                    {
                        await _userManager.AddToRoleAsync(user, role.Name);
                        assignedRoles.Add(role.Name);
                    }
                }
                _logger.LogInformation("İstifadəçiyə rollar təyin edildi: UserId={UserId}, Rollar=[{Roles}]", user.Id, string.Join(", ", assignedRoles));
            }

            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("İstifadəçi uğurla yaradıldı: UserId={UserId}, Email='{Email}', TenantId={TenantId}", user.Id, user.Email, tenantId);

            // Digər modullara yeni user haqqında xəbər ver
            await _eventPublisher.PublishUserCreatedAsync(new UserCreatedIntegrationEvent
            {
                UserId = user.Id,
                TenantId = tenantId,
                Email = user.Email!,
                FullName = user.FullName,
                UserName = user.UserName,
                CreatedAt = user.CreatedAt
            });

            return new UserResponse
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                TenantId = user.TenantId,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                Roles = assignedRoles
            };
        }

        public async Task<List<UserResponse>> GetTenantUsersAsync()
        {
            var tenantId = GetRequiredTenantId();
            _logger.LogInformation("Tenant istifadəçiləri sorğulanır: TenantId={TenantId}", tenantId);

            var users = await _userRepository.GetUsersByTenantAsync(tenantId);

            var result = new List<UserResponse>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                result.Add(new UserResponse
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    FullName = user.FullName,
                    TenantId = user.TenantId,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    Roles = roles.ToList()
                });
            }

            _logger.LogInformation("Tenant istifadəçiləri gətirildi: TenantId={TenantId}, Say={Count}", tenantId, result.Count);
            return result;
        }

        public async Task<UserResponse> GetUserByIdAsync(Guid userId)
        {
            var tenantId = GetRequiredTenantId();
            _logger.LogInformation("İstifadəçi məlumatları sorğulanır: UserId={UserId}, TenantId={TenantId}", userId, tenantId);

            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null || user.TenantId != tenantId)
            {
                _logger.LogWarning("İstifadəçi tapılmadı və ya tenant uyğun gəlmir: UserId={UserId}, TenantId={TenantId}", userId, tenantId);
                throw new NotFoundException($"İstifadəçi tapılmadı.");
            }

            var roles = await _userManager.GetRolesAsync(user);

            return new UserResponse
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                TenantId = user.TenantId,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                Roles = roles.ToList()
            };
        }

        public async Task<UserResponse> UpdateUserAsync(Guid userId, UpdateUserRequest request)
        {
            var tenantId = GetRequiredTenantId();
            _logger.LogInformation("İstifadəçi məlumatları yenilənir: UserId={UserId}, TenantId={TenantId}", userId, tenantId);

            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null || user.TenantId != tenantId)
            {
                _logger.LogWarning("Yenilənəcək istifadəçi tapılmadı: UserId={UserId}, TenantId={TenantId}", userId, tenantId);
                throw new NotFoundException($"İstifadəçi tapılmadı.");
            }

            if (!string.IsNullOrWhiteSpace(request.FullName))
            {
                user.FullName = request.FullName.Trim();
                await _userManager.UpdateAsync(user);
            }

            if (request.RoleIds != null)
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

                var visibleRoles = await _roleRepository.GetVisibleRolesForTenantAsync(tenantId);
                var validRoles = visibleRoles.Where(r => request.RoleIds.Contains(r.Id)).ToList();

                foreach (var role in validRoles)
                {
                    if (!string.IsNullOrWhiteSpace(role.Name))
                    {
                        await _userManager.AddToRoleAsync(user, role.Name);
                    }
                }
                _logger.LogInformation("İstifadəçinin rolları yeniləndi: UserId={UserId}, Yeni rollar sayı={Count}", userId, validRoles.Count);
            }

            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("İstifadəçi məlumatları uğurla yeniləndi: UserId={UserId}", userId);

            var updatedRoles = await _userManager.GetRolesAsync(user);
            return new UserResponse
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                TenantId = user.TenantId,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                Roles = updatedRoles.ToList()
            };
        }

        public async Task ActivateUserAsync(Guid userId)
        {
            var tenantId = GetRequiredTenantId();
            _logger.LogInformation("İstifadəçi aktivləşdirilir: UserId={UserId}, TenantId={TenantId}", userId, tenantId);

            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null || user.TenantId != tenantId)
            {
                _logger.LogWarning("Aktivləşdiriləcək istifadəçi tapılmadı: UserId={UserId}, TenantId={TenantId}", userId, tenantId);
                throw new NotFoundException($"İstifadəçi tapılmadı.");
            }

            await _userRepository.ActivateAsync(userId);
            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("İstifadəçi aktivləşdirildi: UserId={UserId}", userId);
        }

        public async Task DeactivateUserAsync(Guid userId)
        {
            var tenantId = GetRequiredTenantId();
            _logger.LogInformation("İstifadəçi deaktivləşdirilir: UserId={UserId}, TenantId={TenantId}", userId, tenantId);

            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null || user.TenantId != tenantId)
            {
                _logger.LogWarning("Deaktivləşdiriləcək istifadəçi tapılmadı: UserId={UserId}, TenantId={TenantId}", userId, tenantId);
                throw new NotFoundException($"İstifadəçi tapılmadı.");
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("PlatformSuperAdmin") || (user.Email != null && user.Email.Equals("superadmin@altensor.io", StringComparison.OrdinalIgnoreCase)))
            {
                _logger.LogWarning("PlatformSuperAdmin istifadəçisi deaktivləşdirilə bilməz: UserId={UserId}", userId);
                throw new ValidationException("Platform SuperAdmin istifadəçisi sistemin idarəsi üçün qorunur və deaktivləşdirilə bilməz.");
            }

            await _userRepository.DeactivateAsync(userId);
            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("İstifadəçi deaktivləşdirildi: UserId={UserId}", userId);
        }

        public async Task AssignRoleAsync(Guid userId, Guid roleId)
        {
            var tenantId = GetRequiredTenantId();
            _logger.LogInformation("İstifadəçiyə rol təyin edilir: UserId={UserId}, RoleId={RoleId}, TenantId={TenantId}", userId, roleId, tenantId);

            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null || user.TenantId != tenantId)
            {
                _logger.LogWarning("İstifadəçi tapılmadı: UserId={UserId}, TenantId={TenantId}", userId, tenantId);
                throw new NotFoundException($"İstifadəçi tapılmadı.");
            }

            var visibleRoles = await _roleRepository.GetVisibleRolesForTenantAsync(tenantId);
            var role = visibleRoles.FirstOrDefault(r => r.Id == roleId);

            if (role == null || string.IsNullOrWhiteSpace(role.Name))
            {
                _logger.LogWarning("Rol tapılmadı və ya tenanta aid deyil: RoleId={RoleId}, TenantId={TenantId}", roleId, tenantId);
                throw new NotFoundException($"Rol tapılmadı və ya bu müştəriyə aid deyil.");
            }

            await _userManager.AddToRoleAsync(user, role.Name);
            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Rolu istifadəçiyə təyin olundu: UserId={UserId}, RoleName='{RoleName}'", userId, role.Name);
        }

        public async Task RemoveRoleAsync(Guid userId, Guid roleId)
        {
            var tenantId = GetRequiredTenantId();
            _logger.LogInformation("İstifadəçidən rol silinir: UserId={UserId}, RoleId={RoleId}, TenantId={TenantId}", userId, roleId, tenantId);

            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null || user.TenantId != tenantId)
            {
                _logger.LogWarning("İstifadəçi tapılmadı: UserId={UserId}, TenantId={TenantId}", userId, tenantId);
                throw new NotFoundException($"İstifadəçi tapılmadı.");
            }

            var visibleRoles = await _roleRepository.GetVisibleRolesForTenantAsync(tenantId);
            var role = visibleRoles.FirstOrDefault(r => r.Id == roleId);

            if (role == null || string.IsNullOrWhiteSpace(role.Name))
            {
                _logger.LogWarning("Silinəcək rol tapılmadı: RoleId={RoleId}", roleId);
                throw new NotFoundException($"Rol tapılmadı.");
            }

            await _userManager.RemoveFromRoleAsync(user, role.Name);
            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Rol istifadəçidən silindi: UserId={UserId}, RoleName='{RoleName}'", userId, role.Name);
        }
    }
}
