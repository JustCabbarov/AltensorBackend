using Contract.DTOs;
using Contract.Services;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services
{
    public class WorkGroupService : IWorkGroupService
    {
        private readonly IGenericService<WorkGroupDTO, WorkGroup> _groupWorkservice;
        private readonly IAppUserRepository _appUserRepository;
        private readonly IGenericService<TaskDTO, TaskItem> _genericService;
        private readonly INotificationService _notification;
        private readonly IGenericRepository<TaskTransaction> _transaction;

        public WorkGroupService(
            IGenericService<WorkGroupDTO, WorkGroup> groupWorkservice,
            IAppUserRepository appUserRepository,
            IGenericService<TaskDTO, TaskItem> genericService,
            INotificationService notification,
            IGenericRepository<TaskTransaction> transaction)
        {
            _groupWorkservice = groupWorkservice;
            _appUserRepository = appUserRepository;
            _genericService = genericService;
            _notification = notification;
            _transaction = transaction;
        }

        public async Task<IEnumerable<WorkGroupDTO>> GetAllWorkGroupsAsync()
        {
            var data = await _groupWorkservice.GetAllAsync(q => q
                .Include(w => w.Leader)
                .Include(w => w.Users)
                .Include(w => w.Tasks)
            );
            return data;
        }

        public async Task<WorkGroupDTO> GetWorkGroupByIdAsync(Guid id)
        {
            return await _groupWorkservice.GetByIdAsync(id, q => q
                .Include(w => w.Leader)
                .Include(w => w.Users)
                .Include(w => w.Tasks));
        }

        public async Task<WorkGroupDTO> CreateWorkGroupAsync(WorkGroupDTO workGroup)
        {
            return await _groupWorkservice.AddAsync(workGroup);
        }

        public async Task UpdateWorkGroupAsync(WorkGroupDTO workGroup)
        {
            await _groupWorkservice.UpdateAsync(workGroup);
        }

        public async Task DeleteWorkGroupAsync(Guid id)
        {
            await _groupWorkservice.DeleteAsync(id);
        }

        public async Task<WorkGroupDTO?> GetWorkGroupByLeaderIdAsync(string leaderId)
        {
            var leaderGuid = Guid.Parse(leaderId);
            var workGroup = await _groupWorkservice.GetAllAsync(q => q
                .Include(w => w.Leader)
                .Include(w => w.Users)
                .Include(w => w.Tasks)
                .Where(w => w.LeaderId == leaderGuid)
            );
            return workGroup.FirstOrDefault();
        }

        public async Task AssignTaskToGroupAsync(Guid taskId, string leaderId, Guid targetWorkGroupId)
        {
            var currentGroup = await GetWorkGroupByLeaderIdAsync(leaderId);
            if (currentGroup == null)
                throw new Exception("User is not a leader of any work group.");

            var targetWorkGroup = await _groupWorkservice.GetByIdAsync(targetWorkGroupId);
            var task = await _genericService.GetByIdAsync(taskId);
            if (task == null)
                throw new Exception("Task not found.");

            if (task.WorkGroupId != currentGroup.Id)
                throw new Exception("Task does not belong to the leader's work group.");

            task.WorkGroupId = targetWorkGroupId;
            task.Status = CurrentSituation.Pending;

            await _genericService.UpdateAsync(task);

            await _transaction.AddAsync(new TaskTransaction
            {
                TaskItemId = task.Id,
                FromUserId = currentGroup.Id,
                ToUserId = targetWorkGroupId,
                Comments = $"Task assigned from {currentGroup.Name} to {targetWorkGroup.Name}"
            });

            await _notification.NotifyTaskAssignedAsync(targetWorkGroup.LeaderId, task.Title, taskId);
        }

        public async Task AddUserToWorkGroupAsync(Guid workGroupId, string userId)
        {
            var workGroup = await _groupWorkservice.GetByIdAsync(workGroupId);
            if (workGroup == null)
                throw new Exception("Work group not found.");

            var userGuid = Guid.Parse(userId);
            var user = await _appUserRepository.GetByIdAsync(userGuid);
            if (user == null)
                throw new Exception("User not found.");

            workGroup.UserIds ??= new List<string>();
            if (workGroup.UserIds.Any(u => u == userId))
                throw new Exception("User is already a member of the work group.");

            workGroup.UserIds.Add(userId);
            await _groupWorkservice.UpdateAsync(workGroup);
            await _notification.NotifyUserAddedToWorkGroupAsync(userId, workGroup.Name);
        }

        public async Task RemoveUserFromWorkGroupAsync(Guid workGroupId, string userId)
        {
            var workGroup = await _groupWorkservice.GetByIdAsync(workGroupId);
            if (workGroup == null)
                throw new Exception("Work group not found.");

            var userGuid = Guid.Parse(userId);
            var user = await _appUserRepository.GetByIdAsync(userGuid);
            if (user == null)
                throw new Exception("User not found.");

            if (workGroup.UserIds == null || !workGroup.UserIds.Any(u => u == userId))
                throw new Exception("User is not a member of the work group.");

            workGroup.UserIds.Remove(userId);
            await _groupWorkservice.UpdateAsync(workGroup);
            await _notification.NotifyUserRemovedFromWorkGroupAsync(userId, workGroup.Name);
        }
    }
}
