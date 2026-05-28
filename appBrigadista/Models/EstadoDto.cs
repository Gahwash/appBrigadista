using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace appBrigadista.Models
{
    public class EstadoDto
    {
        [JsonPropertyName("modo")]
        public string Modo { get; set; }

        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; }

    }
}
