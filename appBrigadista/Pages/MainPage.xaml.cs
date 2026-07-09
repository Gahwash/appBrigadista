using appBrigadista.Models;
using appBrigadista.PageModels;
using appBrigadista.Services;

namespace appBrigadista.Pages
{
    public partial class MainPage : ContentPage
    {
        private readonly MqttBrigadistaService _mqtt;

        public MainPage(
            MainPageModel model,
            MqttBrigadistaService mqtt)
        {
            InitializeComponent();

            BindingContext = model;

            _mqtt = mqtt;

            // ALERTA RECIBIDA
            _mqtt.AlertaRecibida += OnAlertaRecibida;

            // CAMBIO DE MODO
            _mqtt.ModoActualizado += OnModoActualizado;

            _mqtt.Conectado += () =>
            {
                EstadoMqttLabel.Text = "Conectado al nodo";
            };

            _mqtt.Desconectado += () =>
            {
                EstadoMqttLabel.Text = "No hay conexión al nodo";
            };
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            //ESTADO INICIAL SIEMPRE
            EstadoMqttLabel.Text = "Esperando...";

            try
            {
            
                await _mqtt.ConectarAsync();

            }
            catch (Exception ex)
            {
                EstadoMqttLabel.Text =
                    $"Error de conexión MQTT: {ex.Message}";
            }
        }

        private async void OnAlertaRecibida(
            AlertaMensaje alerta)
        {
            await DisplayAlertAsync(
                "ALERTA",
                alerta.Mensaje,
                "OK");

            await Shell.Current.GoToAsync(
                nameof(EmergenciaPage),
                true,
                new Dictionary<string, object>
                {
                    ["alerta"] = alerta
                });
        }

        private async void OnModoActualizado(string modo)
        {
            if (modo == "NORMAL")
            {
                await Shell.Current.Navigation.PopToRootAsync();
            }
        }
        private void CerrarSesion_Clicked(object sender, EventArgs e)
        {
            //Preferences.Clear();
            TokenService.LimpiarSesion();
            Application.Current!.Windows[0].Page = new NavigationPage(new LoginPage());
        }
    }
}