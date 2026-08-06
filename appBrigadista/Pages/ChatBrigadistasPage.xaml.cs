using System.Collections.ObjectModel;
using appBrigadista.Models;
using appBrigadista.Services;

namespace appBrigadista.Pages
{
    public partial class ChatBrigadistasPage : ContentPage
    {
        private readonly ChatBrigadistasService _chatService;
        private readonly MqttBrigadistaService _mqttService;
        private readonly ObservableCollection<MensajeBrigadista> _mensajes = new();
        private readonly bool _desdeEmergencia;

        private bool _enviandoMensaje = false;

        public ChatBrigadistasPage(
            ChatBrigadistasService chatService,
            MqttBrigadistaService mqttService,
            bool desdeEmergencia)
        {
            InitializeComponent();

            _chatService = chatService;
            _mqttService = mqttService;
            _desdeEmergencia = desdeEmergencia;

            RegresarButton.Text = _desdeEmergencia
                ? "← Emergencia"
                : "← Principal";

            MensajesCollection.ItemsSource = _mensajes;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            _chatService.MensajesActualizados += OnMensajesActualizados;
            _mqttService.MensajeBrigadistaRecibido += OnMensajeBrigadistaRecibido;

            bool historialOk = await _chatService.CargarHistorialAsync();

            if (!historialOk)
            {
                await DisplayAlertAsync(
                    "Chat no disponible",
                    "No fue posible cargar el historial del chat. Verifica que hayas iniciado sesión como brigadista.",
                    "Aceptar");
            }

            await CargarMensajesAsync();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            _chatService.MensajesActualizados -= OnMensajesActualizados;
            _mqttService.MensajeBrigadistaRecibido -= OnMensajeBrigadistaRecibido;
        }

        private void OnMensajeBrigadistaRecibido(MensajeBrigadista mensaje)
        {
            _chatService.RegistrarMensajeEntrante(mensaje);
        }

        private async void OnMensajesActualizados()
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await CargarMensajesAsync();
            });
        }

        private async Task CargarMensajesAsync()
        {
            var mensajes = await _chatService.ObtenerMensajesAsync();

            _mensajes.Clear();

            foreach (var mensaje in mensajes)
            {
                _mensajes.Add(mensaje);
            }

            await ScrollAlUltimoMensajeAsync();
        }

        private async Task ScrollAlUltimoMensajeAsync()
        {
            if (_mensajes.Count == 0)
                return;

            await Task.Delay(100);

            MensajesCollection.ScrollTo(
                _mensajes[^1],
                position: ScrollToPosition.End,
                animate: true);
        }

        private async void OnEnviarClicked(object sender, EventArgs e)
        {
            await EnviarMensajeAsync();
        }

        private async void OnMensajeCompleted(object sender, EventArgs e)
        {
            await EnviarMensajeAsync();
        }

        private async Task EnviarMensajeAsync()
        {
            if (_enviandoMensaje)
                return;

            string contenido = MensajeEntry.Text?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(contenido))
                return;

            _enviandoMensaje = true;

            EnviarButton.IsEnabled = false;
            EnviarButton.Text = "Enviando...";
            MensajeEntry.IsEnabled = false;

            try
            {
                bool enviado = await _chatService.EnviarMensajeGrupalAsync(contenido);

                if (!enviado)
                {
                    await DisplayAlertAsync(
                        "Mensaje no enviado",
                        "No fue posible enviar el mensaje. Verifica tu sesión de brigadista.",
                        "Aceptar");

                    return;
                }

                MensajeEntry.Text = "";
            }
            finally
            {
                _enviandoMensaje = false;

                EnviarButton.Text = "Enviar";
                EnviarButton.IsEnabled = true;
                MensajeEntry.IsEnabled = true;

                MensajeEntry.Focus();
            }
        }

        private async void OnRegresarClicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}