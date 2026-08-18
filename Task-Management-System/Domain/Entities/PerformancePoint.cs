using System;

namespace Domain.Entities
{
    public class PerformancePoint : BaseEntity
    {
        public Guid UserId { get; set; }
        public int Points { get; set; }
        public string Reason { get; set; } = default!;
        public AppUser User { get; set; } = default!;
    }
}
