using System;

namespace Domain.Entities
{
    public class Notification : BaseEntity
    {
        public Guid UserId { get; set; }
        public string Message { get; set; } = default!;
        public bool IsRead { get; set; }
        public Guid? TaskId { get; set; }
        public AppUser User { get; set; } = default!;
    }
}
