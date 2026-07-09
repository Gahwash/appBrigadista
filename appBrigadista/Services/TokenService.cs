using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http.Headers;

namespace appBrigadista.Services
{
    public static class TokenService
    {
        private const string TokenKey = "jwt";

        public static async Task GuardarTokenAsync(string token)
        {
            await SecureStorage.SetAsync(TokenKey, token);
        }

        public static async Task<string> ObtenerTokenAsync()
        {
            return await SecureStorage.GetAsync(TokenKey) ?? "";
        }

        public static void LimpiarSesion()
        {
            SecureStorage.Remove(TokenKey);
            Preferences.Clear();
        }

        public static async Task AgregarAuthorizationAsync(HttpRequestMessage req)
        {
            var token = await ObtenerTokenAsync();

            if (!string.IsNullOrWhiteSpace(token))
            {
                req.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }
    }
}
