using sweet_temptation_clienteEscritorio.vista.producto;
using sweet_temptation_clienteEscritorio.model;
using sweet_temptation_clienteEscritorio.vista.pedido;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace sweet_temptation_clienteEscritorio.vista.cliente
{
    public partial class wPrincipalCliente : Page
    {
        public wPrincipalCliente()
        {
            InitializeComponent();
        }      

        private void CargarNombreUsuario()
        {
            var nombre = App.Current.Properties["Nombre"]?.ToString() ?? "Cliente";
            //txtNombreUsuario.Text = $"{nombre}!";
        }

        private void BtnVerCatalogo_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new wConsultarProductos());
        }

        private void BtnVerPedidos_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Ver Pedidos - Próximamente", 
                "En desarrollo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnCarrito_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Mi Carrito - Próximamente", 
                "En desarrollo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // TODO
        }
    }
}







