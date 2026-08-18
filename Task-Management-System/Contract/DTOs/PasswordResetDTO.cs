using System;
using System.Collections.Generic;
using System.Text;

namespace Contract.DTOs
{
    public class PasswordResetDTO
    {
        public string Email { get; set; }
        public string Token { get; set; }
        public string NewPassword { get; set; }
    }
}
