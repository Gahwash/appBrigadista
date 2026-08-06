//esta clase es la que se encarga de mostrar la informacion de la emergencia, y el pase de lista de las personas afectadas,
//ademas de permitir actualizar el estado de las personas afectadas a "EN_ZONA_SEGURA" o "AUSENTE"

using appBrigadista.Models;
using appBrigadista.Services;

namespace appBrigadista.Pages
{
    [QueryProperty(nameof(Alerta), "alerta")]
    public partial class EmergenciaPage : ContentPage
    {
        private readonly PaseListaService _paseListaService;
        private readonly UbicacionService _ubicacionService;
        private readonly IncidenteService _incidenteService;
        private readonly RadioMapService _radioMapService;
        private readonly DatosMedicosService _datosMedicosService;
        private readonly ChatBrigadistasService _chatService;
        private readonly MqttBrigadistaService _mqtt;

        private AlertaMensaje? _alerta;
        private List<PaseListaEntry> _paseLista = new();
        private PaseListaEntry? _victimaSeleccionada;

        public AlertaMensaje Alerta
        {
            get => _alerta;

            set
            {
                _alerta = value;

                if (_alerta != null)
                {
                    TipoLabel.Text =
                        $"Tipo: {_alerta.Tipo}";

                    SeveridadLabel.Text =
                        $"Severidad: {_alerta.Severidad}";

                    MensajeLabel.Text =
                        _alerta.Mensaje;
                }
            }
        }

        public EmergenciaPage(
            PaseListaService paseListaService,
            UbicacionService ubicacionService,
            IncidenteService incidenteService,
            RadioMapService radioMapService,
            DatosMedicosService datosMedicosService,
            ChatBrigadistasService chatService,
            MqttBrigadistaService mqtt)
        {
            InitializeComponent();

            _paseListaService = paseListaService;
            _ubicacionService = ubicacionService;
            _incidenteService = incidenteService;
            _radioMapService = radioMapService;
            _datosMedicosService = datosMedicosService;
            _chatService = chatService;
            _mqtt = mqtt;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            _paseLista = (await _paseListaService.ObtenerAsync()).ToList();
            PaseListaCollection.ItemsSource = _paseLista;

            await _ubicacionService.CargarUbicacionesAsync();

            _ubicacionService.UbicacionActualizada -= OnUbicacionActualizada;
            _ubicacionService.UbicacionActualizada += OnUbicacionActualizada;

            await _ubicacionService.IniciarMqttAsync();
        }

        protected override async void OnDisappearing()
        {
            base.OnDisappearing();

            _ubicacionService.UbicacionActualizada -= OnUbicacionActualizada;
            await _ubicacionService.DetenerAsync();

            foreach (var persona in _paseLista)
            {
                persona.OcultarUbicacion();
                persona.OcultarDatosMedicos();
            }
            _victimaSeleccionada = null;
        }

        private async void OnVerCroquisClicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new CroquisPage());
        }

        private async void OnSeguroClicked(
            object sender,
            EventArgs e)
        {
            var boton = (Button)sender;

            var persona = (PaseListaEntry)boton.CommandParameter;

            await _paseListaService.ActualizarEstadoAsync(
                persona.VictimaId,
                "EN_ZONA_SEGURA");

            await Refrescar();
        }

        private async void OnAusenteClicked(
            object sender,
            EventArgs e)
        {
            var boton = (Button)sender;

            var persona = (PaseListaEntry)boton.CommandParameter;

            await _paseListaService.ActualizarEstadoAsync(
                persona.VictimaId,
                "AUSENTE");

            await Refrescar();
        }

        private async Task Refrescar()
        {
            _paseLista = (await _paseListaService.ObtenerAsync()).ToList();

            PaseListaCollection.ItemsSource = null;
            PaseListaCollection.ItemsSource = _paseLista;
        }

        private void OnVerUbicacionClicked(object sender, EventArgs e)
        {
            if (sender is not Button boton)
                return;

            if (boton.CommandParameter is not PaseListaEntry persona)
                return;

            // Si la ubicación de esta persona ya está abierta, cerrarla
            if (persona.UbicacionVisible)
            {
                persona.OcultarUbicacion();

                if (_victimaSeleccionada?.VictimaId == persona.VictimaId)
                    _victimaSeleccionada = null;

                return;
            }

            // Cerrar cualquier otra ubicación abierta
            foreach (var item in _paseLista)
            {
                item.OcultarUbicacion();
                item.OcultarDatosMedicos();
            }
            _victimaSeleccionada = persona;

            var ubicacion = _ubicacionService.ObtenerUbicacion(persona.VictimaId);
            persona.MostrarUbicacion(ubicacion);
        }

        private void OnUbicacionActualizada(UbicacionVictima ubicacion)
        {
            var persona = _paseLista
                .FirstOrDefault(p => p.VictimaId == ubicacion.VictimaId);

            if (persona == null)
                return;

            persona.ActualizarUbicacion(ubicacion);
        }


        private async void OnIncidentesClicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(
                new IncidentesPage(
                    _incidenteService,
                    _radioMapService));
        }

        private async void OnVerDatosMedicosClicked(object sender, EventArgs e)
        {
            if (sender is not Button boton)
                return;

            if (boton.CommandParameter is not PaseListaEntry persona)
                return;

            if (persona.DatosMedicosVisible)
            {
                persona.OcultarDatosMedicos();

                if (_victimaSeleccionada?.VictimaId == persona.VictimaId)
                    _victimaSeleccionada = null;

                return;
            }

            foreach (var item in _paseLista)
            {
                item.OcultarUbicacion();
                item.OcultarDatosMedicos();
            }

            _victimaSeleccionada = persona;

            // Abrimos el panel primero, aunque todavía no haya datos.
            persona.MostrarDatosMedicos(null);

            var datos = await _datosMedicosService.ObtenerAsync(persona.VictimaId);

            persona.MostrarDatosMedicos(datos);
        }

        private async void OnChatClicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(
                new ChatBrigadistasPage(
                    _chatService,
                    _mqtt,
                    desdeEmergencia: true));
        }
    }
}
