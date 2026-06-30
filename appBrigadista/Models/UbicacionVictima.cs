using System;
using System.Collections.Generic;
using System.Text;

namespace appBrigadista.Models
{
    public class UbicacionVictima
    {
        public string VictimaId { get; set; } = "";
        public string ZonaId { get; set; } = "";
        public string ZonaNombre { get; set; } = "";
        public int Piso { get; set; } = 0;
        public float Confianza { get; set; } = 0f;
        public long Timestamp { get; set; } = 0;

        public string PisoTexto => Piso == 0 ? "Planta baja"
                                 : Piso > 0 ? $"Piso {Piso}"
                                 : $"Sótano {Math.Abs(Piso)}";

        public string ConfianzaTexto => $"{(int)(Confianza * 100)}%";
    }
}
