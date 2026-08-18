using System;

namespace Domain.Entities
{
    public class TaskCommentMention : BaseEntity
    {
        public Guid CommentId { get; set; }
        public Guid MentionedUserId { get; set; }
        public TaskComment TaskComment { get; set; } = default!;
        public AppUser MentionedUser { get; set; } = default!;
    }
}
