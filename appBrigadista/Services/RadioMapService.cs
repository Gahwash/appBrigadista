using System.Text.Json;
using appBrigadista.Models;

namespace appBrigadista.Services
{
    public class RadioMapService
    {
        private readonly HttpClient _httpClient;

        public RadioMapService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(ApiConfig.BaseUrl)
            };
        }

        public async Task<List<RadioMapEntry>> ObtenerZonasAsync()
        {
            var req = new HttpRequestMessage(HttpMethod.Get, "/api/radiomap");

            await TokenService.AgregarAuthorizationAsync(req);

            var resp = await _httpClient.SendAsync(req);

            if (!resp.IsSuccessStatusCode)
                return new List<RadioMapEntry>();

            var json = await resp.Content.ReadAsStringAsync();

            var lista = JsonSerializer.Deserialize<List<RadioMapEntry>>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<RadioMapEntry>();

            return lista
                .GroupBy(z => $"{z.ZonaId}-{z.ZonaNombre}-{z.Piso}")
                .Select(g => g.First())
                .OrderBy(z => z.Piso)
                .ThenBy(z => z.ZonaNombre)
                .ToList();
        }
    }
}
