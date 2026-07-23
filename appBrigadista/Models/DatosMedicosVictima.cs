using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace appBrigadista.Models
{
    public class DatosMedicosVictima
    {
        [JsonPropertyName("victimaId")]
        public string VictimaId { get; set; } = "";

        [JsonPropertyName("tipoSangre")]
        public string TipoSangre { get; set; } = "";

        [JsonPropertyName("alergias")]
        public string Alergias { get; set; } = "";

        [JsonPropertyName("condicionesMedicas")]
        public string CondicionesMedicas { get; set; } = "";

        [JsonPropertyName("consentimientoLFPDPPP")]
        public bool ConsentimientoLFPDPPP { get; set; }

        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; }

        public string TipoSangreTexto =>
            string.IsNullOrWhiteSpace(TipoSangre)
                ? "Tipo de sangre: no registrado"
                : $"Tipo de sangre: {TipoSangre}";

        public string AlergiasTexto =>
            string.IsNullOrWhiteSpace(Alergias)
                ? "Alergias: no registradas"
                : $"Alergias: {Alergias}";

        public string CondicionesTexto =>
            string.IsNullOrWhiteSpace(CondicionesMedicas)
                ? "Condiciones médicas: no registradas"
                : $"Condiciones médicas: {CondicionesMedicas}";

        public string ConsentimientoTexto =>
            ConsentimientoLFPDPPP
                ? "Consentimiento LFPDPPP: aceptado"
                : "Consentimiento LFPDPPP: no aceptado";

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

                return $"Actualizado: {fecha:HH:mm:ss}";
            }
        }

    }
}
