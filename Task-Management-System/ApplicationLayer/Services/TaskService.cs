using Contract.DTOs;
using Contract.Services;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Application.Services
{
    public class TaskService : ITaksService
    {
        private readonly IGenericService<TaskDTO, TaskItem> _genericService;
        private readonly IAppUserRepository _appUserRepository;
        private readonly IGenericService<TaskCommentDTO, TaskComment> _taskCommentService;
        private readonly IGenericRepository<TaskCommentMention> _commentMentionRepo;
        private readonly IGenericRepository<Notification> _notificationRepo;
        private readonly INotificationService _notification;
        private readonly IGenericRepository<TaskTransaction> _transaction;
        private readonly IUnityOfWork _unityOfWork;
        private readonly ICurrentTenantService _tenantService;

        public TaskService(
            IGenericService<TaskDTO, TaskItem> genericService,
            IAppUserRepository appUserRepository,
            IGenericService<TaskCommentDTO, TaskComment> taskCommentService,
            IGenericRepository<TaskCommentMention> commentMentionRepo,
            INotificationService notification,
            IGenericRepository<Notification> notificationRepo,
            IGenericRepository<TaskTransaction> transaction,
            IUnityOfWork unityOfWork,
            ICurrentTenantService tenantService)
        {
            _genericService = genericService;
            _appUserRepository = appUserRepository;
            _taskCommentService = taskCommentService;
            _commentMentionRepo = commentMentionRepo;
            _notification = notification;
            _notificationRepo = notificationRepo;
            _transaction = transaction;
            _unityOfWork = unityOfWork;
            _tenantService = tenantService;
        }

        public async Task AddComment(Guid taskId, string userId, string comment)
        {
            var task = await _genericService.GetByIdAsync(taskId);
            if (task == null)
                throw new Exception("Task not found");

            var mentionedUsernames = Regex.Matches(comment, @"@(\w+)")
                .Select(m => m.Groups[1].Value)
                .Distinct()
                .ToList();

            var mentionedUserIds = new List<string>();
            if (mentionedUsernames.Any() && _tenantService.TenantId.HasValue)
            {
                var allUsers = await _appUserRepository.GetByTenantAsync(_tenantService.TenantId.Value);
                mentionedUserIds = allUsers
                    .Where(u => u.UserName != null && mentionedUsernames.Contains(u.UserName))
                    .Select(u => u.Id.ToString())
                    .ToList();
            }

            var taskCommentDto = new TaskCommentDTO
            {
                TaskId = taskId,
                UserId = userId,
                Content = comment,
                TaskCommentMentionIDs = mentionedUserIds
            };
            var commentEntity = await _taskCommentService.AddAsync(taskCommentDto);

            foreach (var mentionedUserId in mentionedUserIds)
            {
                if (mentionedUserId == userId) continue;

                var mention = new TaskCommentMention
                {
                    CommentId = commentEntity.Id,
                    MentionedUserId = Guid.Parse(mentionedUserId)
                };
                await _commentMentionRepo.AddAsync(mention);
            }

            var userMessages = new Dictionary<string, string>();
            foreach (var mentionedUserId in mentionedUserIds)
            {
                if (mentionedUserId == userId) continue;
                var message = $"{task.Title} taskına mention edildin: {comment}";
                userMessages.Add(mentionedUserId, message);
            }

            await _notification.NotifyMentionsAsync(userMessages);
        }

        public async Task AcceptTask(Guid taskId, string userId)
        {
            var task = await _genericService.GetByIdAsync(taskId);
            if (task == null)
                throw new Exception("Task not found");
            if (task.AssignedToUserId != userId)
                throw new Exception("You are not assigned to this task.");

            task.Status = CurrentSituation.InProgress;
            await _genericService.UpdateAsync(task);

            await _transaction.AddAsync(new TaskTransaction
            {
                TaskItemId = task.Id,
                FromUserId = Guid.Parse(task.CreatedByUserId),
                ToUserId = Guid.Parse(userId),
                Comments = "Task accepted"
            });

            await _notification.AcceptTaskNotificationAsync(new TaskActionDTO
            {
                userId = task.CreatedByUserId,
                accepterId = task.AssignedToUserId,
                TaskTitle = task.Title
            });
            await _unityOfWork.SaveChangesAsync();
        }

        public async Task RejectTask(Guid taskId, string userId, string reason)
        {
            var task = await _genericService.GetByIdAsync(taskId);
            if (task == null)
                throw new Exception("Task not found");
            if (task.AssignedToUserId != userId)
                throw new Exception("You are not assigned to this task.");

            task.AssignedToUserId = null;
            task.Status = CurrentSituation.Pending;
            await _genericService.UpdateAsync(task);

            await _transaction.AddAsync(new TaskTransaction
            {
                TaskItemId = task.Id,
                FromUserId = Guid.Parse(task.CreatedByUserId),
                ToUserId = Guid.Parse(userId),
                Comments = $"Task rejected: {reason}"
            });

            await _notification.RejectTaskNotificarionAsync(new TaskActionDTO
            {
                userId = task.CreatedByUserId,
                accepterId = userId,
                TaskTitle = task.Title
            });
            await _unityOfWork.SaveChangesAsync();
        }

        public async Task AssignTaskAsync(Guid taskId, string userId)
        {
            var task = await _genericService.GetByIdAsync(taskId);
            if (task == null)
                throw new Exception("Task not found");
            if (task.CreatedByUserId == userId)
                throw new Exception("You cannot assign the task to yourself.");

            task.AssignedToUserId = userId;
            task.Status = CurrentSituation.Assigned;

            await _genericService.UpdateAsync(task);

            await _transaction.AddAsync(new TaskTransaction
            {
                TaskItemId = task.Id,
                FromUserId = Guid.Parse(task.CreatedByUserId),
                ToUserId = Guid.Parse(userId),
                Comments = "Task assigned"
            });

            await _notification.NotifyTaskAssignedAsync(userId, task.Title, taskId);
        }

        public async Task UnAssingTaskAsync(Guid taskId, string userId)
        {
            var task = await _genericService.GetByIdAsync(taskId);
            if (task == null)
                throw new Exception("Task not found");
            if (task.CreatedByUserId != userId)
                throw new Exception("Only the creator can unassign the task.");

            task.AssignedToUserId = null;

            await _transaction.AddAsync(new TaskTransaction
            {
                TaskItemId = task.Id,
                FromUserId = Guid.Parse(userId),
                ToUserId = Guid.Parse(task.CreatedByUserId),
                Comments = "Task unassigned"
            });

            await _genericService.UpdateAsync(task);
        }

        public async Task FinishTask(Guid taskId, string userId)
        {
            var task = await _genericService.GetByIdAsync(taskId);
            if (task == null)
                throw new Exception("Task not found");
            if (task.AssignedToUserId != userId)
                throw new Exception("You are not assigned to this task.");

            task.Status = CurrentSituation.UnderReview;

            await _transaction.AddAsync(new TaskTransaction
            {
                TaskItemId = task.Id,
                FromUserId = Guid.Parse(task.CreatedByUserId),
                ToUserId = Guid.Parse(userId),
                Comments = "Task finished"
            });

            await _genericService.UpdateAsync(task);
            await _notification.FinishTaskNotificationAsync(task.CreatedByUserId, task.Title, taskId);
        }

        public async Task ReturnedForRevision(Guid taskId, string userId, string reason)
        {
            var task = await _genericService.GetByIdAsync(taskId);
            if (task == null)
                throw new Exception("Task not found");
            if (task.CreatedByUserId != userId)
                throw new Exception("Only the creator can return the task for revision.");

            task.Status = CurrentSituation.InProgress;

            await _transaction.AddAsync(new TaskTransaction
            {
                TaskItemId = task.Id,
                FromUserId = Guid.Parse(userId),
                ToUserId = Guid.Parse(task.AssignedToUserId ?? userId),
                Comments = $"Returned for revision: {reason}"
            });

            await _notification.ReturnedForRevision(userId, task.Title, taskId);
            await _genericService.UpdateAsync(task);
        }
    }
}
