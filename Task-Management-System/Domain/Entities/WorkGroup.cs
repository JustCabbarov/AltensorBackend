using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    public class WorkGroup : BaseEntity
    {
        public string Name { get; set; } = default!;
        public Guid LeaderId { get; set; }
        public AppUser Leader { get; set; } = default!;
        public ICollection<AppUser> Users { get; set; } = new List<AppUser>();
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}
