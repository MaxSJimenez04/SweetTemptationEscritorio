using sweet_temptation_clienteEscritorio.vista.Estadisticas;
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
            MessageBox.Show("Estadísticas de Ventas - Próximamente", 
                "En desarrollo", MessageBoxButton.OK, MessageBoxImage.Information);
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
            MessageBox.Show("Gestión de Productos - Próximamente", 
                "En desarrollo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnGestionPedidos_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Gestión de Pedidos - Próximamente", 
                "En desarrollo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}







