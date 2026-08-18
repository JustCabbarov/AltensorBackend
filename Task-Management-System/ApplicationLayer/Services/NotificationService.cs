using Contract.DTOs;
using Contract.Services;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.AspNetCore.SignalR;
using Presentation.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repository;
    private readonly IHubContext<NotificationHub> _hub;
    private readonly IAppUserRepository _appUserRepository;

    public NotificationService(
        INotificationRepository repository,
        IHubContext<NotificationHub> hub,
        IAppUserRepository appUserRepository)
    {
        _repository = repository;
        _hub = hub;
        _appUserRepository = appUserRepository;
    }

    public async Task NotifyTaskAssignedAsync(string userId, string taskTitle, Guid taskId)
    {
        var message = $"Yeni task: {taskTitle} təyin olundu";
        var notification = new Notification
        {
            UserId = Guid.Parse(userId),
            Message = message,
            TaskId = taskId,
            IsRead = false
        };
        await _repository.AddAsync(notification);
        await _hub.Clients.User(userId).SendAsync("ReceiveNotification", new { message, taskId });
    }

    public async Task NotifyMentionsAsync(Dictionary<string, string> userMessages)
    {
        if (userMessages == null || !userMessages.Any()) return;

        var notifications = userMessages
            .Select(kv => new Notification
            {
                UserId = Guid.Parse(kv.Key),
                Message = kv.Value,
                IsRead = false
            })
            .ToList();

        await _repository.AddRangeAsync(notifications);

        var tasks = userMessages
            .Select(kv => _hub.Clients.User(kv.Key).SendAsync("ReceiveNotification", kv.Value))
            .ToList();
        await Task.WhenAll(tasks);
    }

    public async Task<List<Notification>> GetMyNotificationsAsync(string userId)
    {
        return await _repository.GetUserNotificationsAsync(Guid.Parse(userId));
    }

    public async Task MarkReadAsync(Guid id)
    {
        await _repository.MarkAsReadAsync(id);
    }

    public async Task AcceptTaskNotificationAsync(TaskActionDTO task)
    {
        var user = task.accepterId != null
            ? await _appUserRepository.GetByIdAsync(Guid.Parse(task.accepterId))
            : null;

        var message = $"{user?.UserName ?? task.accepterId} {task.TaskTitle} tapşırığını qəbul etdi";
        await _repository.AddAsync(new Notification
        {
            UserId = Guid.Parse(task.accepterId ?? Guid.Empty.ToString()),
            Message = message,
            IsRead = false
        });
        await _hub.Clients.User(task.userId).SendAsync("ReceiveNotification", message);
    }

    public async Task RejectTaskNotificarionAsync(TaskActionDTO task)
    {
        var user = task.accepterId != null
            ? await _appUserRepository.GetByIdAsync(Guid.Parse(task.accepterId))
            : null;

        var message = $"{user?.UserName ?? task.accepterId} {task.TaskTitle} tapşırığını rədd etdi";
        await _repository.AddAsync(new Notification
        {
            UserId = Guid.Parse(task.accepterId ?? Guid.Empty.ToString()),
            Message = message,
            IsRead = false
        });
        await _hub.Clients.User(task.userId).SendAsync("ReceiveNotification", message);
    }

    public async Task NotifyUserAddedToWorkGroupAsync(string userId, string workGroupName)
    {
        var message = $"{workGroupName} iş qrupuna əlavə olundunuz";
        await _repository.AddAsync(new Notification
        {
            UserId = Guid.Parse(userId),
            Message = message,
            IsRead = false
        });
        await _hub.Clients.User(userId).SendAsync("ReceiveNotification", message);
    }

    public async Task NotifyUserRemovedFromWorkGroupAsync(string userId, string workGroupName)
    {
        var message = $"{workGroupName} iş qrupundan çıxarıldınız";
        await _repository.AddAsync(new Notification
        {
            UserId = Guid.Parse(userId),
            Message = message,
            IsRead = false
        });
        await _hub.Clients.User(userId).SendAsync("ReceiveNotification", message);
    }

    public async Task FinishTaskNotificationAsync(string creatorId, string taskTitle, Guid taskId)
    {
        var user = await _appUserRepository.GetByIdAsync(Guid.Parse(creatorId));
        var message = $"{user?.UserName ?? creatorId} {taskTitle} tapşırığı tamamlandı";
        await _repository.AddAsync(new Notification
        {
            UserId = Guid.Parse(creatorId),
            Message = message,
            TaskId = taskId,
            IsRead = false
        });
        await _hub.Clients.User(creatorId).SendAsync("ReceiveNotification", new { message, taskId });
    }

    public Task ReturnedForRevision(string userId, string taskTitle, Guid taskId)
    {
        var message = $"{taskTitle} tapşırığı yenidən işlənmək üçün geri göndərildi";
        var notification = new Notification
        {
            UserId = Guid.Parse(userId),
            Message = message,
            TaskId = taskId,
            IsRead = false
        };
        _repository.AddAsync(notification);
        return _hub.Clients.User(userId).SendAsync("ReceiveNotification", new { message, taskId });
    }
}
