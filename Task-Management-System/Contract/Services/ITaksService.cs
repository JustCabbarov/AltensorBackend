using System;
using System.Threading.Tasks;

namespace Contract.Services
{
    public interface ITaksService
    {
        Task AddComment(Guid taskId, string userId, string comment);
        Task UnAssingTaskAsync(Guid taskId, string userId);
        Task AssignTaskAsync(Guid taskId, string userId);
        Task AcceptTask(Guid taskId, string userId);
        Task RejectTask(Guid taskId, string userId, string reason);
        Task FinishTask(Guid taskId, string userId);
        Task ReturnedForRevision(Guid taskId, string userId, string reason);
    }
}
