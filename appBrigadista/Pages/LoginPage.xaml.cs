using System;
using System.Collections.Generic;
using System.Text;
using appBrigadista.Services;

namespace appBrigadista.Pages
{
    public partial class LoginPage : ContentPage
    {
        private readonly AuthService _authService;

        public LoginPage()
        {
            InitializeComponent();
            _authService = new AuthService();
        }

        private async void LoginButton_Clicked(object sender, EventArgs e)
        {
            ErrorLabel.IsVisible = false;

            string matricula = MatriculaEntry.Text?.Trim() ?? "";
            string password = PasswordEntry.Text?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(matricula))
            {
                ErrorLabel.Text = "Ingresa tu matrícula.";
                ErrorLabel.IsVisible = true;
                return;
            }

            try
            {
                LoginButton.IsEnabled = false;
                LoadingIndicator.IsVisible = true;
                LoadingIndicator.IsRunning = true;

                var usuario = await _authService.LoginAsync(matricula, password);

                if (usuario == null)
                {
                    ErrorLabel.Text = "Usuario no encontrado o error de inicio de sesión.";
                    ErrorLabel.IsVisible = true;
                    return;
                }

                Preferences.Set("usuario_id", usuario.Id);
                Preferences.Set("usuario_identificador", usuario.Identificador);
                Preferences.Set("usuario_nombre", usuario.Nombre);
                Preferences.Set("usuario_rol", usuario.Rol);
                Preferences.Set("sesion_iniciada", true);

                Application.Current!.Windows[0].Page = new AppShell();
            }
            catch (Exception ex)
            {
                ErrorLabel.Text = $"Error de conexión: {ex.Message}";
                ErrorLabel.IsVisible = true;
            }
            finally
            {
                LoginButton.IsEnabled = true;
                LoadingIndicator.IsVisible = false;
                LoadingIndicator.IsRunning = false;
            }
        }
    }
}
