using System;
using System.Collections.Generic;
using System.Text;
using MQTTnet;
using MQTTnet.Client;
using System.Text.Json;
using appBrigadista.Models;

namespace appBrigadista.Services
{
    public class MqttBrigadistaService
    {
        private IMqttClient _client;

        public event Action<AlertaMensaje>? AlertaRecibida;
        public event Action<string>? ModoActualizado;

        //manejar conexion 
        public event Action? Conectado;
        public event Action? Desconectado;

        private static readonly string HOST = ApiConfig.Host;
        private static readonly int PORT = ApiConfig.MqttPort;
        private const string EDIFICIO = "edificioA";
        //bandera para no intentar reconectar si ya se ha conectado una vez, para evitar loops de reconexión
        private bool _conectado = false;

        public async Task ConectarAsync()
        {
            //si ya esta conectado, no se intenta conectar de nuevo
            if (_conectado)
                return;
            var factory = new MqttFactory();

            _client = factory.CreateMqttClient();

            _client.ApplicationMessageReceivedAsync += MensajeRecibidoAsync;

            //manejar eventos de conexión y desconexión para actualizar la UI
            _client.ConnectedAsync += e =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                    Conectado?.Invoke());

                return Task.CompletedTask;
            };

            _client.DisconnectedAsync += e =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                    Desconectado?.Invoke());

                _conectado = false;

                return Task.CompletedTask;
            };//fin de manejar eventos de conexión y desconexión

            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(HOST, PUERTO)
                .WithClientId($"brigadista-{Guid.NewGuid()}")
                .WithCleanSession()
                .Build();

            await _client.ConnectAsync(options);

            await _client.SubscribeAsync($"cinvestav/{EDIFICIO}/alertas");

            await _client.SubscribeAsync($"cinvestav/{EDIFICIO}/estado");

            //después de conectar y suscribirse, se marca como conectado para evitar reconexiones innecesarias
            _conectado = true;
        }

        private Task MensajeRecibidoAsync(MqttApplicationMessageReceivedEventArgs e)
        {
            var topic = e.ApplicationMessage.Topic;

            var payload = Encoding.UTF8.GetString(
                e.ApplicationMessage.PayloadSegment);

            if (topic.EndsWith("/alertas"))
            {
                // IGNORAR MENSAJES RETAINED
                //if (e.ApplicationMessage.Retain)
                //    return Task.CompletedTask;

                var alerta =
                    JsonSerializer.Deserialize<AlertaMensaje>(payload);
                if (alerta?.Tipo =="FIN_EMERGENCIA")
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        ModoActualizado?.Invoke("NORMAL");
                    });

                else if (alerta != null)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        AlertaRecibida?.Invoke(alerta);
                    });
                }
            }
            else if (topic.EndsWith("/estado"))
            {
                var dto =
                    JsonSerializer.Deserialize<EstadoDto>(payload);

                if (dto != null)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        ModoActualizado?.Invoke(dto.Modo);
                    });
                }
            }

            return Task.CompletedTask;
        }

        public async Task DesconectarAsync()
        {
            if (_client != null)
                await _client.DisconnectAsync();
        }
        public void SetModoLocal(string modo)
        {
            MainThread.BeginInvokeOnMainThread(() =>
                ModoActualizado?.Invoke(modo));
        }
    }
}
