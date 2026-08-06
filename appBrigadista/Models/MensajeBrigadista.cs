using System.Text.Json.Serialization;

namespace appBrigadista.Models
{
    public class MensajeBrigadista
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = "";

        [JsonPropertyName("remitenteId")]
        public string RemitenteId { get; set; } = "";

        [JsonPropertyName("remitenteNombre")]
        public string RemitenteNombre { get; set; } = "";

        [JsonPropertyName("destinatarioId")]
        public string DestinatarioId { get; set; } = "";

        [JsonPropertyName("contenido")]
        public string Contenido { get; set; } = "";

        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; }

        [JsonIgnore]
        public string FechaFormateada
        {
            get
            {
                if (Timestamp <= 0)
                    return "";

                var fecha = DateTimeOffset
                    .FromUnixTimeMilliseconds(Timestamp)
                    .ToLocalTime()
                    .DateTime;

                string[] meses =
                {
                    "ene", "feb", "mar", "abr", "may", "jun",
                    "jul", "ago", "sep", "oct", "nov", "dic"
                };

                return $"{fecha.Day:D2}/{meses[fecha.Month - 1]}-{fecha:HH:mm}";
            }
        }

        [JsonIgnore]
        public string HeaderTexto =>
            string.IsNullOrWhiteSpace(RemitenteNombre)
                ? FechaFormateada
                : $"{RemitenteNombre} · {FechaFormateada}";
    }
}
