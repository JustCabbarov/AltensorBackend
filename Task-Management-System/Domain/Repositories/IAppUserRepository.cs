using Domain.Entities;
using System;
using System.Threading.Tasks;

namespace Domain.Repositories
{
    public interface IAppUserRepository
    {
        /// <summary>
        /// Auth Service webhook-dan gələn user məlumatını lokal DB-də yaradır.
        /// User artıq mövcuddursa (eyni Id), yalnız məlumatları yeniləyir.
        /// </summary>
        Task EnsureExistsAsync(Guid userId, Guid tenantId, string email, string? fullName, string? userName);

        Task<AppUser?> GetByIdAsync(Guid userId);
        Task<List<AppUser>> GetByTenantAsync(Guid tenantId);
    }
}
