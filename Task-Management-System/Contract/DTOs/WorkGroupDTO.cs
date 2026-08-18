using System;
using System.Collections.Generic;

namespace Contract.DTOs
{
    public class WorkGroupDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string LeaderId { get; set; } = default!;
        public List<string> UserIds { get; set; } = new List<string>();
        public List<TaskDTO> TaskIds { get; set; } = new List<TaskDTO>();
    }
}
