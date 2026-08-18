using Contract.DTOs;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Contract.Services
{
    public interface INotificationService
    {
        Task NotifyTaskAssignedAsync(string userId, string taskTitle, Guid taskId);
        Task RejectTaskNotificarionAsync(TaskActionDTO dto);
        Task<List<Notification>> GetMyNotificationsAsync(string userId);
        Task MarkReadAsync(Guid id);
        Task NotifyMentionsAsync(Dictionary<string, string> userMessages);
        Task AcceptTaskNotificationAsync(TaskActionDTO dto);
        Task NotifyUserAddedToWorkGroupAsync(string userId, string workGroupName);
        Task NotifyUserRemovedFromWorkGroupAsync(string userId, string workGroupName);
        Task ReturnedForRevision(string userId, string taskTitle, Guid taskId);
        Task FinishTaskNotificationAsync(string userId, string taskTitle, Guid taskId);
    }
}
