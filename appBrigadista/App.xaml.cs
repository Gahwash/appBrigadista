using Microsoft.Extensions.DependencyInjection;


namespace appBrigadista
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

        }



        protected override Window CreateWindow(IActivationState? activationState)
        {
            bool sesionIniciada = Preferences.Get("sesion_iniciada", false);

            if (sesionIniciada)
            {
                return new Window(new AppShell());
            }

            return new Window(new NavigationPage(new LoginPage()));
        }
    }
}