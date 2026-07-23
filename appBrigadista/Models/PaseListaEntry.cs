using System;
using System.Text.Json.Serialization;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace appBrigadista.Models
{
    public class PaseListaEntry : INotifyPropertyChanged
    {
        [JsonPropertyName("victimaId")]
        public string VictimaId { get; set; } = "";

        [JsonPropertyName("nombre")]
        public string Nombre { get; set; } = "";

        [JsonPropertyName("matricula")]
        public string Matricula { get; set; } = "";

        [JsonPropertyName("edificioId")]
        public string EdificioId { get; set; } = "";

        [JsonPropertyName("estado")]
        public string Estado { get; set; } = "";

        [JsonPropertyName("brigadistaIdCambio")]
        public string? BrigadistaId { get; set; }

        [JsonPropertyName("timestampCambioEstado")]
        public long? TsCambio { get; set; }

        private bool _ubicacionVisible;
        private bool _datosMedicosVisible;

        [JsonIgnore]
        public bool UbicacionVisible
        {
            get => _ubicacionVisible;
            set
            {
                if (_ubicacionVisible == value)
                    return;

                _ubicacionVisible = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TextoBotonUbicacion));
            }
        }

        [JsonIgnore]
        public UbicacionVictima? UbicacionActual { get; private set; }

        [JsonIgnore]
        public string TextoBotonUbicacion =>
            UbicacionVisible ? "Cerrar" : "Ver ubicación";

        [JsonIgnore]
        public string ZonaNombreTexto =>
            UbicacionActual?.ZonaNombre ?? "Sin datos de ubicación";

        [JsonIgnore]
        public string PisoTexto =>
            UbicacionActual?.PisoTexto ?? "";

        [JsonIgnore]
        public string ConfianzaTexto =>
            UbicacionActual == null ? "" : $"Confianza: {UbicacionActual.ConfianzaTexto}";

        [JsonIgnore]
        public string UltimaActualizacionTexto
        {
            get
            {
                if (UbicacionActual == null || UbicacionActual.Timestamp <= 0)
                    return "";

                var fecha = DateTimeOffset
                    .FromUnixTimeMilliseconds(UbicacionActual.Timestamp)
                    .ToLocalTime()
                    .DateTime;

                return $"Última actualización: {fecha:HH:mm:ss}";
            }
        }

        [JsonIgnore]
        public bool DatosMedicosVisible
        {
            get => _datosMedicosVisible;
            set
            {
                if (_datosMedicosVisible == value)
                    return;

                _datosMedicosVisible = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TextoBotonDatosMedicos));
            }
        }

        [JsonIgnore]
        public DatosMedicosVictima? DatosMedicosActual { get; private set; }

        [JsonIgnore]
        public string TextoBotonDatosMedicos =>
            DatosMedicosVisible ? "Cerrar" : "Datos médicos";

        [JsonIgnore]
        public string TipoSangreTexto =>
            DatosMedicosActual?.TipoSangreTexto ?? "Tipo de sangre: no disponible";

        [JsonIgnore]
        public string AlergiasTexto =>
            DatosMedicosActual?.AlergiasTexto ?? "Alergias: no disponibles";

        [JsonIgnore]
        public string CondicionesTexto =>
            DatosMedicosActual?.CondicionesTexto ?? "Condiciones médicas: no disponibles";

        [JsonIgnore]
        public string ConsentimientoTexto =>
            DatosMedicosActual?.ConsentimientoTexto ?? "Consentimiento: no disponible";

        [JsonIgnore]
        public string FechaDatosMedicosTexto =>
            DatosMedicosActual?.FechaTexto ?? "";

        public void ActualizarUbicacion(UbicacionVictima? ubicacion)
        {
            UbicacionActual = ubicacion;

            OnPropertyChanged(nameof(ZonaNombreTexto));
            OnPropertyChanged(nameof(PisoTexto));
            OnPropertyChanged(nameof(ConfianzaTexto));
            OnPropertyChanged(nameof(UltimaActualizacionTexto));
        }

        public void MostrarUbicacion(UbicacionVictima? ubicacion)
        {
            ActualizarUbicacion(ubicacion);
            UbicacionVisible = true;
        }

        public void OcultarUbicacion()
        {
            UbicacionVisible = false;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void ActualizarDatosMedicos(DatosMedicosVictima? datos)
        {
            DatosMedicosActual = datos;

            OnPropertyChanged(nameof(TipoSangreTexto));
            OnPropertyChanged(nameof(AlergiasTexto));
            OnPropertyChanged(nameof(CondicionesTexto));
            OnPropertyChanged(nameof(ConsentimientoTexto));
            OnPropertyChanged(nameof(FechaDatosMedicosTexto));
        }

        public void MostrarDatosMedicos(DatosMedicosVictima? datos)
        {
            ActualizarDatosMedicos(datos);
            DatosMedicosVisible = true;
        }

        public void OcultarDatosMedicos()
        {
            DatosMedicosVisible = false;
        }
    }
}
