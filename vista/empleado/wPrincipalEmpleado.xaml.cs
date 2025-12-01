using sweet_temptation_clienteEscritorio.vista.pedido;
using System.Windows;
using System.Windows.Controls;

namespace sweet_temptation_clienteEscritorio.vista.empleado
{
    public partial class wPrincipalEmpleado : Page
    {
        public wPrincipalEmpleado()
        {
            InitializeComponent();
            CargarNombreUsuario();
        }

        private void CargarNombreUsuario()
        {
            var nombre = App.Current.Properties["Nombre"]?.ToString() ?? "Empleado";
            txtNombreUsuario.Text = $"{nombre}!";
        }

        private void BtnGestionarProductos_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Gestionar Productos - Próximamente", 
                "En desarrollo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnGestionarPedidos_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new wPedidos());
        }

        private void BtnProductos_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Ver Productos - Próximamente", 
                "En desarrollo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnPedidosRecientes_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Pedidos Recientes - Próximamente", 
                "En desarrollo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}

