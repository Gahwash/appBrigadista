using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace appBrigadista.Models
{
    public class PaseListaEntry
    {
        [JsonPropertyName("victimaId")]
        public string VictimaId { get; set; }

        [JsonPropertyName("nombre")]
        public string Nombre { get; set; }

        [JsonPropertyName("matricula")]
        public string Matricula { get; set; }

        [JsonPropertyName("edificioId")]
        public string EdificioId { get; set; }

        [JsonPropertyName("estado")]
        public string Estado { get; set; }

        [JsonPropertyName("brigadistaIdCambio")]
        public string? BrigadistaId { get; set; }

        [JsonPropertyName("timestampCambioEstado")]
        public long? TsCambio { get; set; }
    }
}
