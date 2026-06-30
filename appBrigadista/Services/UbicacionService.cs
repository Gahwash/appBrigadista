using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using appBrigadista.Models;
using MQTTnet;
using MQTTnet.Client;

namespace appBrigadista.Services
{
    public class UbicacionService
    {
        private readonly Dictionary<string, UbicacionVictima> _ubicaciones = new();
        private readonly HttpClient _httpClient;
        private IMqttClient? _mqttClient;

        public event Action<UbicacionVictima>? UbicacionActualizada;

        public UbicacionService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(ApiConfig.BaseUrl)
            };
        }

        public async Task<List<UbicacionVictima>> CargarUbicacionesAsync()
        {
            try
            {
                var req = new HttpRequestMessage(HttpMethod.Get, "/api/ubicacion");
                req.Headers.Add("X-Role", "BRIGADISTA");

                var resp = await _httpClient.SendAsync(req);

                if (!resp.IsSuccessStatusCode)
                    return new List<UbicacionVictima>();

                var json = await resp.Content.ReadAsStringAsync();

                var lista = JsonSerializer.Deserialize<List<UbicacionVictima>>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<UbicacionVictima>();

                foreach (var ubicacion in lista)
                    _ubicaciones[ubicacion.VictimaId] = ubicacion;

                return lista;
            }
            catch
            {
                return new List<UbicacionVictima>();
            }
        }

        public UbicacionVictima? ObtenerUbicacion(string victimaId)
        {
            return _ubicaciones.TryGetValue(victimaId, out var ubicacion)
                ? ubicacion
                : null;
        }

        public async Task IniciarMqttAsync()
        {
            if (_mqttClient?.IsConnected == true)
                return;

            var factory = new MqttFactory();
            _mqttClient = factory.CreateMqttClient();

            _mqttClient.ApplicationMessageReceivedAsync += OnMensajeRecibido;

            var clientId = $"brigadista-ubicacion-{Guid.NewGuid().ToString("N")[..6]}";

            var opciones = new MqttClientOptionsBuilder()
                .WithTcpServer(ApiConfig.Host, ApiConfig.MqttPort)
                .WithClientId(clientId)
                .WithCleanSession()
                .Build();

            try
            {
                await _mqttClient.ConnectAsync(opciones);
                await _mqttClient.SubscribeAsync("cinvestav/edificioA/ubicaciones");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error MQTT ubicaciones: {ex.Message}");
            }
        }

        private Task OnMensajeRecibido(MqttApplicationMessageReceivedEventArgs e)
        {
            var payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);

            try
            {
                using var doc = JsonDocument.Parse(payload);
                var root = doc.RootElement;

                var ubicacion = new UbicacionVictima
                {
                    VictimaId = GetString(root, "victimaId"),
                    ZonaId = GetString(root, "zonaId"),
                    ZonaNombre = GetString(root, "zonaNombre"),
                    Piso = GetInt(root, "piso"),
                    Confianza = GetFloat(root, "confianza"),
                    Timestamp = GetLong(root, "timestamp")
                };

                // El mensaje MQTT puede venir como "zona" en lugar de "zonaNombre"
                if (string.IsNullOrWhiteSpace(ubicacion.ZonaNombre))
                    ubicacion.ZonaNombre = GetString(root, "zona");

                if (!string.IsNullOrWhiteSpace(ubicacion.VictimaId))
                {
                    _ubicaciones[ubicacion.VictimaId] = ubicacion;

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        UbicacionActualizada?.Invoke(ubicacion);
                    });
                }
            }
            catch
            {
                // Ignorar mensajes mal formados para no romper la app
            }

            return Task.CompletedTask;
        }

        public async Task DetenerAsync()
        {
            if (_mqttClient?.IsConnected == true)
                await _mqttClient.DisconnectAsync();
        }

        private static string GetString(JsonElement root, string name)
        {
            return root.TryGetProperty(name, out var prop) &&
                   prop.ValueKind == JsonValueKind.String
                ? prop.GetString() ?? ""
                : "";
        }

        private static int GetInt(JsonElement root, string name)
        {
            return root.TryGetProperty(name, out var prop) &&
                   prop.TryGetInt32(out var value)
                ? value
                : 0;
        }

        private static long GetLong(JsonElement root, string name)
        {
            return root.TryGetProperty(name, out var prop) &&
                   prop.TryGetInt64(out var value)
                ? value
                : 0;
        }

        private static float GetFloat(JsonElement root, string name)
        {
            return root.TryGetProperty(name, out var prop) &&
                   prop.TryGetSingle(out var value)
                ? value
                : 0f;
        }
    }
}