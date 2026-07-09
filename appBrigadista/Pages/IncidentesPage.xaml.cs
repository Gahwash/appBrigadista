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

        private async void OnRegresarClicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}
