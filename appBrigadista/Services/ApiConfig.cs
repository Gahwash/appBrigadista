using System;
using System.Collections.Generic;
using System.Text;

namespace appBrigadista.Services
{
    public class ApiConfig
    {
        // Solo cambias esta IP cuando cambie el nodo FOG
        public const string Host = "192.168.1.23";

        public const int ApiPort = 8080;
        public const int MqttPort = 1883;

        public static string BaseUrl => $"http://{Host}:{ApiPort}";
    }
}
