using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http.Json;
using System.Text.Json;
using appBrigadista.Models;

namespace appBrigadista.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;

        public AuthService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(ApiConfig.BaseUrl)
            };
        }

        public async Task<UsuarioSesion?> LoginAsync(string matricula, string password)
        {
            var body = new LoginRequest
            {
                Identificador = matricula,
                Password = password
            };

            var response = await _httpClient.PostAsJsonAsync("/api/auth/login", body);

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<UsuarioSesion>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
    }
}
