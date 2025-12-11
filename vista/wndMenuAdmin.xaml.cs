using sweet_temptation_clienteEscritorio.vista.admin;
using sweet_temptation_clienteEscritorio.vista.pedido;
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

        private void BtnEstadisticasClick(object sender, RoutedEventArgs e)
        {
            fmPrincipal.Navigate(new wSeleccionEstadisticas());
        }

        private void BtnCuentasClick(object sender, RoutedEventArgs e)
        {
            fmPrincipal.Navigate(new wGestionCuentas());
        }

        private void BtnProductosClick(object sender, RoutedEventArgs e)
        {

        }

        private void BtnPedidosClick(object sender, RoutedEventArgs e)
        {
            fmPrincipal.Navigate(new wHistorialPedidos());
        }

        private void BtnHomeClick(object sender, RoutedEventArgs e)
        {
            fmPrincipal.Navigate(new wPrincipalAdmin());
        }
    }
}
