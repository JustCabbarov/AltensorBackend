using System;

namespace Contract.DTOs
{
    public class PerformancePointDTo
    {
        public string userId { get; set; } = default!;
        public Guid taskId { get; set; }
        public string reason { get; set; } = default!;
        public string senderId { get; set; } = default!;
    }
}
