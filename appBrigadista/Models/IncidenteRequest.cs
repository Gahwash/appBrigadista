using System.Text.Json.Serialization;

namespace appBrigadista.Models
{
    public class IncidenteRequest
    {
        [JsonPropertyName("tipo")]
        public string Tipo { get; set; } = "";

        [JsonPropertyName("descripcion")]
        public string Descripcion { get; set; } = "";

        [JsonPropertyName("reportadoPorId")]
        public string ReportadoPorId { get; set; } = "";

        [JsonPropertyName("zonaId")]
        public string ZonaId { get; set; } = "";

        [JsonPropertyName("zonaNombre")]
        public string ZonaNombre { get; set; } = "";

        [JsonPropertyName("piso")]
        public int Piso { get; set; }

        [JsonPropertyName("confianzaUbicacion")]
        public float ConfianzaUbicacion { get; set; }
    }
}
