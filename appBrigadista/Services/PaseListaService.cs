using appBrigadista.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace appBrigadista.Services
{
    public class PaseListaService
    {
        private const string BASE = "http://192.168.10.229:8080";
        private readonly HttpClient _http = new();
       
        public async Task<List<PaseListaEntry>> ObtenerAsync()
        {
            var req = new HttpRequestMessage(HttpMethod.Get, $"{BASE}/api/pase-lista");
            req.Headers.Add("X-Role", "BRIGADISTA");
            var res = await _http.SendAsync(req);
            var json = await res.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<PaseListaEntry>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true})
                   ?? new List<PaseListaEntry>();
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

        public async Task ActualizarEstadoAsync(string victimaId, string nuevoEstado)
        {
            var body = JsonSerializer.Serialize(new
            {
                estado = nuevoEstado,
                brigadistaId = "brigadista-001"
            });
            var req = new HttpRequestMessage(
                HttpMethod.Put, $"{BASE}/api/pase-lista/{victimaId}/estado");
            req.Headers.Add("X-Role", "BRIGADISTA");
            req.Content = new StringContent(body, Encoding.UTF8, "application/json");
            await _http.SendAsync(req);
        }
    }
}
