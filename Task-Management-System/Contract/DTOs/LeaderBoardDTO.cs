using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contract.DTOs
{
    public class LeaderBoardDTO
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public int TotalPoints { get; set; }
    }

}
