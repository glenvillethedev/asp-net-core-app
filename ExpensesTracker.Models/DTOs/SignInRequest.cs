using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpensesTracker.Services.DTOs
{
    public class SignInRequest
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
        public bool IsPersistent { get; set; }
    }
}
