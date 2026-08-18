using System;

namespace Domain.Entities
{
    public class TaskTransaction : BaseEntity
    {
        public Guid TaskItemId { get; set; }
        public Guid FromUserId { get; set; }
        public Guid ToUserId { get; set; }
        public string Comments { get; set; } = default!;

        public TaskItem TaskItem { get; set; } = default!;
        public AppUser FromUser { get; set; } = default!;
        public AppUser ToUser { get; set; } = default!;
    }
}
