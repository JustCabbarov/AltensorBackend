using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string EmployeeId { get; set; }
        public ICollection<TaskItem> AssignedTasks { get; set; }
        public ICollection<TaskItem> CreatedTasks { get; set; }
        [JsonIgnore]
        public ICollection<PerformancePoint> PerformancePoints { get; set; }
        public ICollection<TaskComment> TaskComments { get; set; }
        public ICollection<TaskCommentMention> TaskCommentMentions { get; set; }
        public ICollection<Notification> Notifications { get; set; }
        public int? WorkGroupId { get; set; }
        public WorkGroup? WorkGroup { get; set; }



    }
}
