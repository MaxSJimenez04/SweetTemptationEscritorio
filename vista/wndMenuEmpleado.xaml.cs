using sweet_temptation_clienteEscritorio.vista.empleado;
using sweet_temptation_clienteEscritorio.vista.pedido;
using sweet_temptation_clienteEscritorio.vista.producto;
using System.Windows;

namespace sweet_temptation_clienteEscritorio.vista
{
    public partial class wndMenuEmpleado : Window
    {
        public wndMenuEmpleado()
        {
            InitializeComponent();
            fmPrincipal.Navigate(new wPrincipalEmpleado());
        }

        private void BtnClickCerrarSesion(object sender, RoutedEventArgs e)
        {
            App.Current.Properties.Clear();
            new wndLogin().Show();
            this.Close();
        }

        private void BtnInventarioClick(object sender, RoutedEventArgs e)
        {
            fmPrincipal.Navigate(new wAdministrarProductos());
        }

        private void BtnHistorialClick(object sender, RoutedEventArgs e)
        {
            fmPrincipal.Navigate(new wHistorialPedidos());
        }

        private void BtnPedidosClick(object sender, RoutedEventArgs e)
        {
            fmPrincipal.Navigate(new wPedidos());
        }
    }
}
