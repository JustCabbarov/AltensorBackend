using System;

namespace Contract.DTOs
{
    public record TaskCommentMentionDTO
    {
        public Guid CommentId { get; set; }
        public string MentionedUserId { get; set; } = default!;
    }
}
