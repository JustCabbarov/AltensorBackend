using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    /// <summary>
    /// Auth Service-dən sinxronizasiya edilən readonly user entity.
    /// Bu cədvəl birbaşa yazılmır — yalnız /internal/webhooks/user-created endpoint vasitəsilə doldurulur.
    /// </summary>
    public class AppUser
    {
        public Guid Id { get; set; }          // Auth Service-dəki UserId ilə eynidir
        public Guid TenantId { get; set; }
        public string Email { get; set; } = default!;
        public string? FullName { get; set; }
        public string? UserName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();
        public ICollection<TaskItem> CreatedTasks { get; set; } = new List<TaskItem>();
        public ICollection<TaskComment> TaskComments { get; set; } = new List<TaskComment>();
        public ICollection<TaskCommentMention> TaskCommentMentions { get; set; } = new List<TaskCommentMention>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<PerformancePoint> PerformancePoints { get; set; } = new List<PerformancePoint>();
        public Guid? WorkGroupId { get; set; }
        public WorkGroup? WorkGroup { get; set; }
    }
}
