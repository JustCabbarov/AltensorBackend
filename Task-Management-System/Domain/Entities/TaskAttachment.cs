using System;

namespace Domain.Entities
{
    public class TaskAttachment : BaseEntity
    {
        public Guid TaskId { get; set; }
        public TaskItem Task { get; set; } = default!;

        public string FileName { get; set; } = default!;
        public string ObjectName { get; set; } = default!;
        public string ContentType { get; set; } = default!;
        public long Size { get; set; }
    }
}
