using appBrigadista.Models;
using appBrigadista.PageModels;
using appBrigadista.Services;

namespace appBrigadista.Pages
{
    public partial class MainPage : ContentPage
    {
        private readonly MqttBrigadistaService _mqtt;
        private readonly ChatBrigadistasService _chatService;

        private bool _cerrandoPorFinEmergencia = false;
        private bool _navegandoAEmergencia = false;
        private static bool _aperturaEmergenciaGlobal = false;

        public MainPage(
            MainPageModel model,
            MqttBrigadistaService mqtt,
            ChatBrigadistasService chatService)
        {
            InitializeComponent();

            BindingContext = model;

            _mqtt = mqtt;
            _chatService = chatService;

            _mqtt.AlertaRecibida += OnAlertaRecibida;
            _mqtt.ModoActualizado += OnModoActualizado;

            _mqtt.Conectado += () =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    EstadoMqttLabel.Text = "Conectado al nodo FOG";
                });
            };

            _mqtt.Desconectado += () =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    EstadoMqttLabel.Text = "No hay conexión con el nodo FOG";
                });
            };
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (_mqtt.EstaConectado)
            {
                EstadoMqttLabel.Text = "Conectado al nodo FOG";
                return;
            }

            EstadoMqttLabel.Text = "Conectando con el nodo FOG...";

            try
            {
                await _mqtt.ConectarAsync();

                EstadoMqttLabel.Text = _mqtt.EstaConectado
                    ? "Conectado al nodo FOG"
                    : "Conectando con el nodo FOG...";
            }
            catch (Exception ex)
            {
                EstadoMqttLabel.Text =
                    $"Error de conexión MQTT: {ex.Message}";
            }
        }

        private async void OnAlertaRecibida(AlertaMensaje alerta)
        {
            if (alerta == null)
                return;

            if (_navegandoAEmergencia || _aperturaEmergenciaGlobal)
                return;

            _navegandoAEmergencia = true;
            _aperturaEmergenciaGlobal = true;

            try
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    var shell = Shell.Current;

                    if (shell == null)
                        return;

                    var paginaActual = shell.CurrentPage;

                    if (paginaActual is EmergenciaPage)
                        return;

                    if (paginaActual != null)
                    {
                        try
                        {
                            await paginaActual.DisplayAlertAsync(
                                "Alerta sísmica",
                                alerta.Mensaje ?? "Se recibió una alerta de emergencia.",
                                "Aceptar");
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine(
                                $"No se pudo mostrar alerta: {ex.Message}");
                        }
                    }

                    await shell.GoToAsync(
                        nameof(EmergenciaPage),
                        true,
                        new Dictionary<string, object>
                        {
                            ["alerta"] = alerta
                        });
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Error al abrir emergencia: {ex.Message}");
            }
            finally
            {
                _navegandoAEmergencia = false;
                _aperturaEmergenciaGlobal = false;
            }
        }

        private async void OnModoActualizado(string modo)
        {
            if (modo != "NORMAL")
                return;

            if (_cerrandoPorFinEmergencia)
                return;

            _cerrandoPorFinEmergencia = true;

            try
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    var nav = Shell.Current.Navigation;

                    while (nav.ModalStack.Count > 0)
                    {
                        await nav.PopModalAsync(false);
                    }

                    await nav.PopToRootAsync(false);

                    EstadoMqttLabel.Text = "Conectado al nodo FOG";
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Error al cerrar emergencia: {ex.Message}");
            }
            finally
            {
                _cerrandoPorFinEmergencia = false;
            }
        }

        private void CerrarSesion_Clicked(object sender, EventArgs e)
        {
            TokenService.LimpiarSesion();

            Application.Current!.Windows[0].Page =
                new NavigationPage(new LoginPage());
        }

        private async void OnChatClicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(
                new ChatBrigadistasPage(
                    _chatService,
                    _mqtt,
                    desdeEmergencia: false));
        }
    }
}