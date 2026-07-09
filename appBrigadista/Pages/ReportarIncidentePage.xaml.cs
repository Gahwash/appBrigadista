using appBrigadista.Models;
using appBrigadista.Services;

namespace appBrigadista.Pages
{
    public partial class ReportarIncidentePage : ContentPage
    {
        private readonly IncidenteService _incidenteService;
        private readonly RadioMapService _radioMapService;

        private List<RadioMapEntry> _zonas = new();

        private string _tipoSeleccionado = "";
        private RadioMapEntry? _zonaSeleccionada;

        private readonly string[] _tiposIncidente =
        {
            "Obstáculo en ruta de evacuación",
            "Salida bloqueada",
            "Escaleras bloqueadas",
            "Persona necesita asistencia",
            "Persona lesionada",
            "Persona atrapada",
            "Humo o fuego",
            "Daño estructural",
            "Riesgo eléctrico",
            "Fuga de gas o agua",
            "Zona con aglomeración o pánico",
            "Persona no localizada",
            "Otro"
        };

        public ReportarIncidentePage(
            IncidenteService incidenteService,
            RadioMapService radioMapService)
        {
            InitializeComponent();

            _incidenteService = incidenteService;
            _radioMapService = radioMapService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await CargarZonasAsync();
        }

        private async Task CargarZonasAsync()
        {
            _zonas = await _radioMapService.ObtenerZonasAsync();

            if (_zonas.Count == 0)
            {
                await DisplayAlertAsync(
                    "Sin ubicaciones",
                    "No se encontraron zonas calibradas en el nodo.",
                    "Aceptar");
            }
        }

        private async void OnSeleccionarTipoTapped(object sender, TappedEventArgs e)
        {
            string? opcion = await DisplayActionSheetAsync(
                "Tipo de incidente",
                "Cancelar",
                null,
                _tiposIncidente);

            if (string.IsNullOrWhiteSpace(opcion) || opcion == "Cancelar")
                return;

            _tipoSeleccionado = opcion;

            TipoSeleccionadoLabel.Text = opcion;
            TipoSeleccionadoLabel.TextColor = Color.FromArgb("#212121");
        }

        private async void OnSeleccionarZonaTapped(object sender, TappedEventArgs e)
        {
            if (_zonas.Count == 0)
            {
                await DisplayAlertAsync(
                    "Sin ubicaciones",
                    "No hay ubicaciones calibradas disponibles.",
                    "Aceptar");
                return;
            }

            var opciones = _zonas
                .Select(z => z.NombreMostrar)
                .ToArray();

            string? opcion = await DisplayActionSheetAsync(
                "Ubicación del incidente",
                "Cancelar",
                null,
                opciones);

            if (string.IsNullOrWhiteSpace(opcion) || opcion == "Cancelar")
                return;

            _zonaSeleccionada = _zonas
                .FirstOrDefault(z => z.NombreMostrar == opcion);

            if (_zonaSeleccionada == null)
                return;

            ZonaSeleccionadaLabel.Text = _zonaSeleccionada.NombreMostrar;
            ZonaSeleccionadaLabel.TextColor = Color.FromArgb("#212121");
        }

        private async void OnEnviarClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_tipoSeleccionado))
            {
                await DisplayAlertAsync(
                    "Falta información",
                    "Selecciona un tipo de incidente.",
                    "Aceptar");
                return;
            }

            if (_zonaSeleccionada == null)
            {
                await DisplayAlertAsync(
                    "Falta ubicación",
                    "Selecciona la ubicación donde ocurrió el incidente.",
                    "Aceptar");
                return;
            }

            string usuarioId = Preferences.Get("usuario_id", "");

            if (string.IsNullOrWhiteSpace(usuarioId))
            {
                await DisplayAlertAsync(
                    "Sesión no disponible",
                    "No se encontró el identificador del usuario.",
                    "Aceptar");
                return;
            }

            var incidente = new IncidenteRequest
            {
                Tipo = _tipoSeleccionado,
                Descripcion = DescripcionEditor.Text?.Trim() ?? "",
                ReportadoPorId = usuarioId,
                ZonaId = _zonaSeleccionada.ZonaId,
                ZonaNombre = _zonaSeleccionada.ZonaNombre,
                Piso = _zonaSeleccionada.Piso,
                ConfianzaUbicacion = 1.0f
            };

            bool creado = await _incidenteService.CrearAsync(incidente);

            if (!creado)
            {
                await DisplayAlertAsync(
                    "Error",
                    "No fue posible enviar el reporte.",
                    "Aceptar");
                return;
            }

            await DisplayAlertAsync(
                "Reporte enviado",
                "El incidente fue reportado correctamente.",
                "Aceptar");

            await Navigation.PopModalAsync();
        }

        private async void OnCancelarClicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}
