using Altensorcrm.Domain.Entity;

namespace Altensorcrm.Domain.Repository;

public interface ITaskRepository : IGenericRepository<TaskItem>
{
    Task<IReadOnlyList<TaskItem>> GetTasksByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
