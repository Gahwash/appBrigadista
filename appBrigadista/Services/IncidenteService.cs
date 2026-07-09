using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using appBrigadista.Models;

namespace appBrigadista.Services
{
    public class IncidenteService
    {
        private readonly HttpClient _httpClient;

        public IncidenteService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(ApiConfig.BaseUrl)
            };
        }

        public async Task<List<IncidenteResponse>> ObtenerAsync()
        {
            var req = new HttpRequestMessage(HttpMethod.Get, "/api/incidentes");

            await TokenService.AgregarAuthorizationAsync(req);

            var resp = await _httpClient.SendAsync(req);

            if (resp.StatusCode == HttpStatusCode.Unauthorized)
            {
                TokenService.LimpiarSesion();
                return new List<IncidenteResponse>();
            }

            if (!resp.IsSuccessStatusCode)
                return new List<IncidenteResponse>();

            var json = await resp.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<List<IncidenteResponse>>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<IncidenteResponse>();
        }

        public async Task<bool> CrearAsync(IncidenteRequest incidente)
        {
            var req = new HttpRequestMessage(HttpMethod.Post, "/api/incidentes")
            {
                Content = JsonContent.Create(incidente)
            };

            await TokenService.AgregarAuthorizationAsync(req);

            var resp = await _httpClient.SendAsync(req);

            if (resp.StatusCode == HttpStatusCode.Unauthorized)
            {
                TokenService.LimpiarSesion();
                return false;
            }

            return resp.IsSuccessStatusCode;
        }

        public async Task<bool> CambiarEstadoAsync(string id, string estado)
        {
            var req = new HttpRequestMessage(HttpMethod.Patch, $"/api/incidentes/{id}/estado")
            {
                Content = JsonContent.Create(new
                {
                    estado
                })
            };

            await TokenService.AgregarAuthorizationAsync(req);

            var resp = await _httpClient.SendAsync(req);

            if (resp.StatusCode == HttpStatusCode.Unauthorized)
            {
                TokenService.LimpiarSesion();
                return false;
            }

            return resp.IsSuccessStatusCode;
        }
    }
}
