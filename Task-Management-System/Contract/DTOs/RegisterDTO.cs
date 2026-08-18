using System;
using System.Collections.Generic;
using System.Text;

namespace Contract.DTOs
{
    public record RegisterDTO
    {
        public string? Email { get; set; }
        public string Password { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
