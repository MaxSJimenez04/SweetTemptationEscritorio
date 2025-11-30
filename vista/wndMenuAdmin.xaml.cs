using sweet_temptation_clienteEscritorio.vista.admin;
using System.Windows;

namespace sweet_temptation_clienteEscritorio.vista
{
    public partial class wndMenuAdmin : Window
    {
        public wndMenuAdmin()
        {
            InitializeComponent();
            fmPrincipal.Navigate(new wPrincipalAdmin());
        }

        private void BtnInicio_Click(object sender, RoutedEventArgs e)
        {
            if (fmPrincipal.CanGoBack)
            {
                fmPrincipal.GoBack();
            }
            else
            {
                fmPrincipal.Navigate(new wPrincipalAdmin());
            }
        }

        private void BtnGestionCuentas_Click(object sender, RoutedEventArgs e)
        {
            fmPrincipal.Navigate(new wGestionCuentas());
        }

        private void BtnClickCerrarSesion(object sender, RoutedEventArgs e)
        {
            App.Current.Properties.Clear();
            new wndLogin().Show();
            this.Close();
        }
    }
}
