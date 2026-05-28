
//esta clase es la que se encarga de mostrar la informacion de la emergencia, y el pase de lista de las personas afectadas,
//ademas de permitir actualizar el estado de las personas afectadas a "EN_ZONA_SEGURA" o "AUSENTE"
using System;
using System.Collections.Generic;
using System.Text;
using appBrigadista.Models;
using appBrigadista.Services;

namespace appBrigadista.Pages
{
    [QueryProperty(nameof(Alerta), "alerta")]
    public partial class EmergenciaPage : ContentPage
    {
        private readonly PaseListaService _paseListaService;

        private AlertaMensaje _alerta;

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
            PaseListaService paseListaService)
        {
            InitializeComponent();

            _paseListaService = paseListaService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            var lista =
                await _paseListaService.ObtenerAsync();

            PaseListaCollection.ItemsSource = lista;
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

            Refrescar();
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

            Refrescar();
        }

        private async Task Refrescar()
        {
            var lista =
                await _paseListaService.ObtenerAsync();

            PaseListaCollection.ItemsSource = null;

            PaseListaCollection.ItemsSource =
                lista.ToList();
        }
    }
}
