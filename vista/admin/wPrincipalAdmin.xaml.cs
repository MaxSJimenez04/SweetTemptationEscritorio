using sweet_temptation_clienteEscritorio.vista.Estadisticas;
using sweet_temptation_clienteEscritorio.vista.pedido;
using sweet_temptation_clienteEscritorio.vista.producto;
using System.Windows;
using System.Windows.Controls;

namespace sweet_temptation_clienteEscritorio.vista.admin
{
    public partial class wPrincipalAdmin : Page
    {
        public wPrincipalAdmin()
        {
            InitializeComponent();
            CargarNombreUsuario();
        }

        private void CargarNombreUsuario()
        {
            var nombre = App.Current.Properties["Nombre"]?.ToString() ?? "Administrador";
            txtNombreUsuario.Text = $"{nombre}!";
        }

        private void BtnEstadisticasVenta_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new wEstadisticasVentas());
        }

        private void BtnEstadisticasProductos_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new wEstadisticasProductos());
        }

        private void BtnGestionCuentas_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new wGestionCuentas());
        }

        private void BtnGestionProductos_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new wAdministrarProductos());
        }

        private void BtnGestionPedidos_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new wHistorialPedidos());
        }
    }
}







