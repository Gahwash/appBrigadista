using System.Text.Json.Serialization;

namespace appBrigadista.Models
{
    public class RadioMapEntry
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("zonaId")]
        public string ZonaId { get; set; } = "";

        [JsonPropertyName("zonaNombre")]
        public string ZonaNombre { get; set; } = "";

        [JsonPropertyName("piso")]
        public int Piso { get; set; }

        [JsonPropertyName("fingerprint")]
        public string Fingerprint { get; set; } = "";

        public string PisoTexto => Piso == 0 ? "Planta baja"
                                 : Piso > 0 ? $"Piso {Piso}"
                                 : $"Sótano {Math.Abs(Piso)}";

        public string NombreMostrar => $"{ZonaNombre} - {PisoTexto}";
    }
}
