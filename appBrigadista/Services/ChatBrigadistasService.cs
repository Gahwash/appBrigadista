using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using appBrigadista.Models;

namespace appBrigadista.Services
{
    public class ChatBrigadistasService
    {
        private readonly HttpClient _httpClient;
        private readonly List<MensajeBrigadista> _mensajes = new();
        private readonly object _lock = new();

        private const int LIMITE_MENSAJES = 100;

        public event Action? MensajesActualizados;

        public ChatBrigadistasService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(ApiConfig.BaseUrl)
            };
        }

        public async Task<bool> CargarHistorialAsync()
        {
            try
            {
                var req = new HttpRequestMessage(
                    HttpMethod.Get,
                    "/api/mensajes");

                await TokenService.AgregarAuthorizationAsync(req);

                var resp = await _httpClient.SendAsync(req);
                var json = await resp.Content.ReadAsStringAsync();

                Debug.WriteLine("====== CHAT HISTORIAL ======");
                Debug.WriteLine($"URL: {req.RequestUri}");
                Debug.WriteLine($"Status: {(int)resp.StatusCode} {resp.StatusCode}");
                Debug.WriteLine($"Respuesta: {json}");
                Debug.WriteLine("============================");

                if (resp.StatusCode == HttpStatusCode.Unauthorized)
                {
                    TokenService.LimpiarSesion();
                    return false;
                }

                if (!resp.IsSuccessStatusCode)
                    return false;

                var mensajes = JsonSerializer.Deserialize<List<MensajeBrigadista>>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<MensajeBrigadista>();

                lock (_lock)
                {
                    _mensajes.Clear();

                    _mensajes.AddRange(
                        mensajes
                            .OrderBy(m => m.Timestamp)
                            .TakeLast(LIMITE_MENSAJES));
                }

                MensajesActualizados?.Invoke();

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al cargar historial del chat: {ex.Message}");
                return false;
            }
        }

        public Task<List<MensajeBrigadista>> ObtenerMensajesAsync()
        {
            lock (_lock)
            {
                var copia = _mensajes
                    .OrderBy(m => m.Timestamp)
                    .TakeLast(LIMITE_MENSAJES)
                    .ToList();

                return Task.FromResult(copia);
            }
        }

        public async Task<bool> EnviarMensajeGrupalAsync(string contenido)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(contenido))
                    return false;

                string remitenteNombre = Preferences.Get(
                    "usuario_nombre",
                    "Brigadista");

                var body = new
                {
                    destinatarioId = "",
                    remitenteNombre = remitenteNombre,
                    contenido = contenido.Trim()
                };

                var req = new HttpRequestMessage(
                    HttpMethod.Post,
                    "/api/mensajes")
                {
                    Content = JsonContent.Create(body)
                };

                await TokenService.AgregarAuthorizationAsync(req);

                var resp = await _httpClient.SendAsync(req);
                var json = await resp.Content.ReadAsStringAsync();

                Debug.WriteLine("====== CHAT ENVIAR ======");
                Debug.WriteLine($"URL: {req.RequestUri}");
                Debug.WriteLine($"Status: {(int)resp.StatusCode} {resp.StatusCode}");
                Debug.WriteLine($"Respuesta: {json}");
                Debug.WriteLine("=========================");

                if (resp.StatusCode == HttpStatusCode.Unauthorized)
                {
                    TokenService.LimpiarSesion();
                    return false;
                }

                if (!resp.IsSuccessStatusCode)
                    return false;

                var mensaje = JsonSerializer.Deserialize<MensajeBrigadista>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (mensaje != null)
                    RegistrarMensajeEntrante(mensaje);

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al enviar mensaje de chat: {ex.Message}");
                return false;
            }
        }

        public void RegistrarMensajeEntrante(MensajeBrigadista mensaje)
        {
            if (mensaje == null)
                return;

            if (string.IsNullOrWhiteSpace(mensaje.Id))
                mensaje.Id = Guid.NewGuid().ToString();

            if (mensaje.Timestamp <= 0)
                mensaje.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            lock (_lock)
            {
                bool existe = _mensajes.Any(m => m.Id == mensaje.Id);

                if (!existe)
                    _mensajes.Add(mensaje);

                var ultimos = _mensajes
                    .OrderBy(m => m.Timestamp)
                    .TakeLast(LIMITE_MENSAJES)
                    .ToList();

                _mensajes.Clear();
                _mensajes.AddRange(ultimos);
            }

            MensajesActualizados?.Invoke();
        }
    }
}