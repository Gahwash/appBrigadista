using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;


namespace appBrigadista.Models
{
    public class AlertaMensaje
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("tipo")]
        public string Tipo { get; set; }

        [JsonPropertyName("severidad")]
        public string Severidad { get; set; }

        [JsonPropertyName("mensaje")]
        public string Mensaje { get; set; }

        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; }
    }
}
