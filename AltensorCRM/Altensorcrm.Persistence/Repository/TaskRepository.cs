using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Altensorcrm.Contract.Services.Tenant;
using Altensorcrm.Domain.Entity;
using Altensorcrm.Domain.Repository;
using Altensorcrm.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Altensorcrm.Persistence.Repository;

public class TaskRepository : GenericRepository<TaskItem>, ITaskRepository
{
    public TaskRepository(AppDbContext context, ICurrentTenantService tenantService)
        : base(context, tenantService)
    {
    }

    public async Task<IReadOnlyList<TaskItem>> GetTasksByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await ApplyTenantFilter(DbSet.AsNoTracking())
            .Include(t => t.Checklists)
            .Include(t => t.Comments)
            .Where(t => t.AssignedUserId == userId)
            .OrderBy(t => t.DueDate)
            .ToListAsync(cancellationToken);
    }
}

