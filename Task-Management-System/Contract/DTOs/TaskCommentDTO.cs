using System;
using System.Collections.Generic;

namespace Contract.DTOs
{
    public class TaskCommentDTO
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = default!;
        public string UserId { get; set; } = default!;
        public Guid TaskId { get; set; }
        public List<string>? TaskCommentMentionIDs { get; set; }
    }
}
