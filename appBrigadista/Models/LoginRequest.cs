using System;
using System.Collections.Generic;
using System.Text;

namespace appBrigadista.Models
{
    public class LoginRequest
    {
        public string Identificador { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
