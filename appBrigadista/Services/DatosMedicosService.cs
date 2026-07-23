using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Net;
using System.Text.Json;
using appBrigadista.Models;

namespace appBrigadista.Services
{
    public class DatosMedicosService
    {
        private readonly HttpClient _httpClient;

        public DatosMedicosService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(ApiConfig.BaseUrl)
            };
        }

        public async Task<DatosMedicosVictima?> ObtenerAsync(string victimaId)
        {
            try
            {
                var req = new HttpRequestMessage(
                    HttpMethod.Get,
                    $"/api/salud/{victimaId}");

                await TokenService.AgregarAuthorizationAsync(req);

                var resp = await _httpClient.SendAsync(req);
                var json = await resp.Content.ReadAsStringAsync();

                Debug.WriteLine("====== DATOS MEDICOS ======");
                Debug.WriteLine($"URL: {req.RequestUri}");
                Debug.WriteLine($"Status: {(int)resp.StatusCode} {resp.StatusCode}");
                Debug.WriteLine($"Respuesta: {json}");
                Debug.WriteLine("===========================");

                if (resp.StatusCode == HttpStatusCode.Unauthorized)
                {
                    TokenService.LimpiarSesion();
                    return null;
                }

                if (!resp.IsSuccessStatusCode)
                    return null;

                if (string.IsNullOrWhiteSpace(json))
                    return null;

                return JsonSerializer.Deserialize<DatosMedicosVictima>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al obtener datos médicos: {ex.Message}");
                return null;
            }
        }
    }
}
