using System.Text.Json.Serialization;

namespace appBrigadista.Models
{
    public class IncidenteResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = "";

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

        [JsonPropertyName("estado")]
        public string Estado { get; set; } = "";

        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; }

        public string PisoTexto => Piso == 0 ? "Planta baja"
                                 : Piso > 0 ? $"Piso {Piso}"
                                 : $"Sótano {Math.Abs(Piso)}";

        public string FechaTexto
        {
            get
            {
                if (Timestamp <= 0)
                    return "";

                var fecha = DateTimeOffset
                    .FromUnixTimeMilliseconds(Timestamp)
                    .ToLocalTime()
                    .DateTime;

                return fecha.ToString("HH:mm:ss");
            }
        }
    }
}