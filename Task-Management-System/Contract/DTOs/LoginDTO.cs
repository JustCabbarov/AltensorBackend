using System;
using System.Collections.Generic;
using System.Text;

namespace Contract.DTOs
{
        public record LoginDTO
        {
            public string Email { get; set; }
            public string Password { get; set; }

        }
    
}
