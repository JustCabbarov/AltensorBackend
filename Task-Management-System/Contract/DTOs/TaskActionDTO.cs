using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contract.DTOs
{
    public class TaskActionDTO
    {
        public string userId { get; set; }
        public string TaskTitle { get; set; }
        public string accepterId { get; set; }
    }
}
