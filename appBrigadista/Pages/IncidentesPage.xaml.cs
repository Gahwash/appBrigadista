using appBrigadista.Services;

namespace appBrigadista.Pages
{
    public partial class IncidentesPage : ContentPage
    {
        private readonly IncidenteService _incidenteService;
        private readonly RadioMapService _radioMapService;

        public IncidentesPage(
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

            await CargarIncidentesAsync();
        }

        private async Task CargarIncidentesAsync()
        {
            var incidentes = await _incidenteService.ObtenerAsync();

            IncidentesCollection.ItemsSource = incidentes
                .OrderByDescending(i => i.Timestamp)
                .ToList();
        }

        private async void OnActualizarClicked(object sender, EventArgs e)
        {
            await CargarIncidentesAsync();
        }

        private async void OnReportarIncidenteClicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(
                new ReportarIncidentePage(
                    _incidenteService,
                    _radioMapService));
        }

        private async void OnEnRevisionClicked(object sender, EventArgs e)
        {
            await CambiarEstadoDesdeBotonAsync(sender, "EN_REVISION");
        }

        private async void OnResueltoClicked(object sender, EventArgs e)
        {
            await CambiarEstadoDesdeBotonAsync(sender, "RESUELTO");
        }

        private async Task CambiarEstadoDesdeBotonAsync(object sender, string estado)
        {
            if (sender is not Button boton)
                return;

            if (boton.CommandParameter is not string id ||
                string.IsNullOrWhiteSpace(id))
                return;

            boton.IsEnabled = false;

            try
            {
                bool ok = await _incidenteService.CambiarEstadoAsync(id, estado);

                if (!ok)
                {
                    await DisplayAlertAsync(
                        "Error",
                        "No fue posible actualizar el estado del incidente.",
                        "Aceptar");
                    return;
                }

                await CargarIncidentesAsync();
            }
            finally
            {
                boton.IsEnabled = true;
            }
        }
        private async void OnRegresarClicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}
