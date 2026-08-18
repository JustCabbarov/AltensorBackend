using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    public class TaskComment : BaseEntity
    {
        public string Content { get; set; } = default!;
        public Guid UserId { get; set; }
        public AppUser User { get; set; } = default!;
        public Guid TaskId { get; set; }
        public TaskItem TaskItem { get; set; } = default!;

        public ICollection<TaskCommentMention> TaskCommentMentions { get; set; } = new List<TaskCommentMention>();
    }
}
