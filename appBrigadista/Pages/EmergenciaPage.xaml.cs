
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
        private readonly UbicacionService _ubicacionService; //sprint 3

        private AlertaMensaje _alerta;
        private PaseListaEntry? _victimaSeleccionada;//sprint 3
        private Button? _botonUbicacionActual;

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
            UbicacionService ubicacionService)//constructor que recibe los servicios de pase de lista y ubicacion(sp3)
        {
            InitializeComponent();

            _paseListaService = paseListaService;
            _ubicacionService = ubicacionService;//sprint 3
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            var lista = await _paseListaService.ObtenerAsync();
            PaseListaCollection.ItemsSource = lista;

            await _ubicacionService.CargarUbicacionesAsync();

            _ubicacionService.UbicacionActualizada -= OnUbicacionActualizada;//sprint 3
            _ubicacionService.UbicacionActualizada += OnUbicacionActualizada;//sprint 3

            await _ubicacionService.IniciarMqttAsync();
        }

        protected override async void OnDisappearing()
        {
            base.OnDisappearing();

            _ubicacionService.UbicacionActualizada -= OnUbicacionActualizada;
            await _ubicacionService.DetenerAsync();

            if (_botonUbicacionActual != null)
                _botonUbicacionActual.Text = "Ver ubicación";

            _botonUbicacionActual = null;
            _victimaSeleccionada = null;

            OcultarUbicacion();
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
        //metodos de ubicacion
        private void OnVerUbicacionClicked(object sender, EventArgs e)
        {
            if (sender is not Button boton)
                return;

            if (boton.CommandParameter is not PaseListaEntry persona)
                return;

            // Si ya está abierta la ubicación de esta misma víctima, cerrar
            if (_victimaSeleccionada?.VictimaId == persona.VictimaId && UbicacionFrame.IsVisible)
            {
                OcultarUbicacion();
                boton.Text = "Ver ubicación";
                _botonUbicacionActual = null;
                _victimaSeleccionada = null;
                return;
            }

            // Si había otro botón abierto, regresarlo a "Ver ubicación"
            if (_botonUbicacionActual != null)
                _botonUbicacionActual.Text = "Ver ubicación";

            // Abrir ubicación de la nueva víctima
            _victimaSeleccionada = persona;
            _botonUbicacionActual = boton;
            boton.Text = "Cerrar";

            var ubicacion = _ubicacionService.ObtenerUbicacion(persona.VictimaId);
            MostrarUbicacion(persona, ubicacion);
        }
        private void OcultarUbicacion()
        {
            UbicacionFrame.IsVisible = false;

            VictimaUbicacionLabel.Text = "";
            ZonaNombreLabel.Text = "";
            PisoLabel.Text = "";
            ConfianzaLabel.Text = "";
            UltimaActualizacionLabel.Text = "";
        }

        private void OnUbicacionActualizada(UbicacionVictima ubicacion)
        {
            if (_victimaSeleccionada == null)
                return;

            if (ubicacion.VictimaId != _victimaSeleccionada.VictimaId)
                return;

            MostrarUbicacion(_victimaSeleccionada, ubicacion);
        }

        private void MostrarUbicacion(PaseListaEntry victima, UbicacionVictima? ubicacion)
        {
            UbicacionFrame.IsVisible = true;
            VictimaUbicacionLabel.Text = victima.Nombre;

            if (ubicacion == null)
            {
                ZonaNombreLabel.Text = "Sin datos de ubicación";
                PisoLabel.Text = "";
                ConfianzaLabel.Text = "";
                UltimaActualizacionLabel.Text = "";
                return;
            }

            ZonaNombreLabel.Text = ubicacion.ZonaNombre;
            PisoLabel.Text = ubicacion.PisoTexto;
            ConfianzaLabel.Text = $"Confianza: {ubicacion.ConfianzaTexto}";

            if (ubicacion.Timestamp > 0)
            {
                var fecha = DateTimeOffset
                    .FromUnixTimeMilliseconds(ubicacion.Timestamp)
                    .ToLocalTime()
                    .DateTime;

                UltimaActualizacionLabel.Text = $"Última actualización: {fecha:HH:mm:ss}";
            }
            else
            {
                UltimaActualizacionLabel.Text = "";
            }
        }
    }
}
