using System;
using System.Collections.Generic;
using System.Text;

namespace appBrigadista.Pages
{
    public partial class CroquisPage : ContentPage
    {
        public CroquisPage()
        {
            InitializeComponent();
        }

        private async void OnCerrarClicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}
