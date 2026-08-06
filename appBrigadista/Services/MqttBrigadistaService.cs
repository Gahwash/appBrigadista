using System.Text;
using System.Text.Json;
using appBrigadista.Models;
using MQTTnet;
using MQTTnet.Client;

namespace appBrigadista.Services
{
    public class MqttBrigadistaService
    {
        private IMqttClient? _client;

        public event Action<AlertaMensaje>? AlertaRecibida;
        public event Action<string>? ModoActualizado;
        

        public event Action? Conectado;
        public event Action? Desconectado;

        public event Action<MensajeBrigadista>? MensajeBrigadistaRecibido;

        private static readonly string HOST = ApiConfig.Host;
        private static readonly int PORT = ApiConfig.MqttPort;
        private const string EDIFICIO = "edificioA";

        public bool EstaConectado => _client?.IsConnected == true;

        public async Task ConectarAsync()
        {
            if (EstaConectado)
                return;

            var factory = new MqttFactory();

            _client = factory.CreateMqttClient();

            _client.ApplicationMessageReceivedAsync += MensajeRecibidoAsync;

            _client.ConnectedAsync += e =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Conectado?.Invoke();
                });

                return Task.CompletedTask;
            };

            _client.DisconnectedAsync += e =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Desconectado?.Invoke();
                });

                return Task.CompletedTask;
            };

            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(HOST, PORT)
                .WithClientId($"brigadista-{Guid.NewGuid():N}")
                .WithCleanSession()
                .Build();

            await _client.ConnectAsync(options);

            await _client.SubscribeAsync($"cinvestav/{EDIFICIO}/alertas");
            await _client.SubscribeAsync($"cinvestav/{EDIFICIO}/estado");
            await _client.SubscribeAsync($"cinvestav/{EDIFICIO}/brigadistas");
        }

        private Task MensajeRecibidoAsync(MqttApplicationMessageReceivedEventArgs e)
        {
            try
            {
                var topic = e.ApplicationMessage.Topic;

                var payload = Encoding.UTF8.GetString(
                    e.ApplicationMessage.PayloadSegment);

                if (string.IsNullOrWhiteSpace(payload))
                    return Task.CompletedTask;

                if (topic.EndsWith("/alertas"))
                {
                    var alerta = JsonSerializer.Deserialize<AlertaMensaje>(
                        payload,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                    if (alerta == null)
                        return Task.CompletedTask;

                    if (alerta.Tipo == "FIN_EMERGENCIA")
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            ModoActualizado?.Invoke("NORMAL");
                        });

                        return Task.CompletedTask;
                    }

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        AlertaRecibida?.Invoke(alerta);
                    });
                }
                else if (topic.EndsWith("/estado"))
                {
                    var dto = JsonSerializer.Deserialize<EstadoDto>(
                        payload,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                    if (dto == null)
                        return Task.CompletedTask;

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        ModoActualizado?.Invoke(dto.Modo);
                    });
                }
                else if (topic == $"cinvestav/{EDIFICIO}/brigadistas")
                {
                    var mensaje = JsonSerializer.Deserialize<MensajeBrigadista>(
                        payload,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                    if (mensaje == null)
                        return Task.CompletedTask;

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        MensajeBrigadistaRecibido?.Invoke(mensaje);
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Error al procesar mensaje MQTT: {ex.Message}");
            }

            return Task.CompletedTask;
        }

        public async Task DesconectarAsync()
        {
            if (_client?.IsConnected == true)
            {
                await _client.DisconnectAsync();
            }
        }

        public void SetModoLocal(string modo)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ModoActualizado?.Invoke(modo);
            });
        }
    }
}
