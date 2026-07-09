using System;
using System.Collections.Generic;
using System.Text;

namespace appBrigadista.Models
{
    public class UsuarioSesion
    {
        public string Id { get; set; } = string.Empty;
        public string Identificador { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}
