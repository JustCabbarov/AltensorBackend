using Contract.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Contract.Services
{
    public interface IWorkGroupService
    {
        Task AssignTaskToGroupAsync(Guid taskId, string leaderId, Guid targetWorkGroupId);
        Task<WorkGroupDTO?> GetWorkGroupByLeaderIdAsync(string leaderId);
        Task DeleteWorkGroupAsync(Guid id);
        Task UpdateWorkGroupAsync(WorkGroupDTO workGroup);
        Task<WorkGroupDTO> CreateWorkGroupAsync(WorkGroupDTO workGroup);
        Task<WorkGroupDTO> GetWorkGroupByIdAsync(Guid id);
        Task<IEnumerable<WorkGroupDTO>> GetAllWorkGroupsAsync();
        Task AddUserToWorkGroupAsync(Guid workGroupId, string userId);
        Task RemoveUserFromWorkGroupAsync(Guid workGroupId, string userId);
    }
}
