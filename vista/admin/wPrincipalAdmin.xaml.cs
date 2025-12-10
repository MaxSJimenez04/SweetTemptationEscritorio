using sweet_temptation_clienteEscritorio.vista.Estadisticas;
<<<<<<< HEAD
using sweet_temptation_clienteEscritorio.vista.pedido;
using sweet_temptation_clienteEscritorio.vista.producto;
=======
>>>>>>> e524a3d (Para estadisticas de ventas y correccion)
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
<<<<<<< HEAD
            NavigationService.Navigate(new wEstadisticasVentas());
=======
            NavigationService?.Navigate(new wEstadisticasVentas());
>>>>>>> e524a3d (Para estadisticas de ventas y correccion)
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
<<<<<<< HEAD
            NavigationService.Navigate(new wAdministrarProductos());
=======
            var ventana = Window.GetWindow(this) as wndMenuAdmin;

            if (ventana != null)
            {
                ventana.fmPrincipal.Navigate(new sweet_temptation_clienteEscritorio.vista.producto.wAdministrarProductos());
            }
>>>>>>> e524a3d (Para estadisticas de ventas y correccion)
        }

        private void BtnGestionPedidos_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new wHistorialPedidos());
        }
    }
}







