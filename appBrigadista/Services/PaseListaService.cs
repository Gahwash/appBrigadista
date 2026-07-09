using appBrigadista.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;
using appBrigadista.Models;
using System.Diagnostics;

namespace appBrigadista.Services
{
    public class PaseListaService
    {
        private static readonly string BASE = ApiConfig.BaseUrl;
        private readonly HttpClient _http = new();

        public async Task<List<PaseListaEntry>> ObtenerAsync()
        {
            try
            {
                var req = new HttpRequestMessage(
                    HttpMethod.Get,
                    $"{BASE}/api/pase-lista");

                await TokenService.AgregarAuthorizationAsync(req);

                // Temporal: el nodo todavía lo pide en PaseListaController
                req.Headers.Add("X-Role", "BRIGADISTA");

                var res = await _http.SendAsync(req);
                var json = await res.Content.ReadAsStringAsync();

                Debug.WriteLine("====== PASE LISTA ======");
                Debug.WriteLine($"URL: {req.RequestUri}");
                Debug.WriteLine($"Status: {(int)res.StatusCode} {res.StatusCode}");
                Debug.WriteLine($"Respuesta: {json}");
                Debug.WriteLine("========================");

                if (!res.IsSuccessStatusCode)
                {
                    Console.WriteLine(
                        $"Error al obtener pase de lista. Código: {(int)res.StatusCode}. Respuesta: {json}");

                    return new List<PaseListaEntry>();
                }

                if (string.IsNullOrWhiteSpace(json))
                {
                    Console.WriteLine("El nodo respondió vacío en /api/pase-lista.");
                    return new List<PaseListaEntry>();
                }

                return JsonSerializer.Deserialize<List<PaseListaEntry>>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<PaseListaEntry>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error de conexión con el nodo FOG: {ex.Message}");
                return new List<PaseListaEntry>();
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error al leer JSON del pase de lista: {ex.Message}");
                return new List<PaseListaEntry>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inesperado en PaseListaService.ObtenerAsync: {ex.Message}");
                return new List<PaseListaEntry>();
            }
        }

        ////mock de pase de lista 
        //// MOCK EN MEMORIA
        //private List<PaseListaEntry> _mockLista =
        //    new()
        //    {
        //        new()
        //        {
        //            VictimaId = "victima-001",
        //            Nombre = "Diego Aguilar",
        //            Matricula = "A01234567",
        //            Estado = "PRESENTE"
        //        },

        //        new()
        //        {
        //            VictimaId = "victima-002",
        //            Nombre = "Julio Rocha",
        //            Matricula = "A07654321",
        //            Estado = "PRESENTE"
        //        },

        //        new()
        //        {
        //            VictimaId = "victima-003",
        //            Nombre = "Invitado Externo",
        //            Matricula = "EXT-001",
        //            Estado = "PRESENTE"
        //        }
        //    };
        //public async Task<List<PaseListaEntry>> ObtenerAsync()
        //{
        //    await Task.Delay(300);

        //    return _mockLista;
        //}

        //public async Task ActualizarEstadoAsync(
        //    string victimaId,
        //    string nuevoEstado)
        //{
        //    await Task.Delay(200);

        //    var persona = _mockLista.FirstOrDefault(
        //        x => x.VictimaId == victimaId);

        //    if (persona != null)
        //    {
        //        persona.Estado = nuevoEstado;
        //    }
        //}

        public async Task<bool> ActualizarEstadoAsync(
    string victimaId,
    string estado)
        {
            try
            {
                var body = JsonSerializer.Serialize(new
                {
                    estado
                });

                var req = new HttpRequestMessage(
                    HttpMethod.Put,
                    $"{BASE}/api/pase-lista/{victimaId}/estado");

                await TokenService.AgregarAuthorizationAsync(req);

                // Temporal: el nodo todavía lo pide en PaseListaController
                req.Headers.Add("X-Role", "BRIGADISTA");

                req.Content = new StringContent(
                    body,
                    Encoding.UTF8,
                    "application/json");

                var res = await _http.SendAsync(req);
                var respuesta = await res.Content.ReadAsStringAsync();

                if (!res.IsSuccessStatusCode)
                {
                    Console.WriteLine(
                        $"Error al actualizar estado. Código: {(int)res.StatusCode}. Respuesta: {respuesta}");

                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en ActualizarEstadoAsync: {ex.Message}");
                return false;
            }
        }
    }
}
