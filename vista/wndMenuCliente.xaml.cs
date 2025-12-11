using sweet_temptation_clienteEscritorio.model;
using sweet_temptation_clienteEscritorio.vista.cliente;
using sweet_temptation_clienteEscritorio.vista.pedido;
using sweet_temptation_clienteEscritorio.vista.producto;
using System.Windows;
using System.Windows.Navigation;

namespace sweet_temptation_clienteEscritorio.vista
{
    public partial class wndMenuCliente : Window
    {
        public wndMenuCliente()
        {
            InitializeComponent();
            fmPrincipal.Navigate(new wPrincipalCliente());
        }
        private void fmPrincipal_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e) {

        }

         private void BtnClickInicio(object sender, RoutedEventArgs e) {
            if(fmPrincipal.CanGoBack) {
                fmPrincipal.GoBack();
            } else {
                fmPrincipal.Navigate(new wPrincipalCliente());
            }
        }
        private void btnClickCarrito(object sender, RoutedEventArgs e) {
            Pedido pedido = new Pedido();
            pedido.id = 0;
            fmPrincipal.NavigationService.Navigate(new wPedido(pedido));
        }
        private void btnClickProductos(object sender, RoutedEventArgs e) {
            if(fmPrincipal.NavigationService != null) {
                fmPrincipal.NavigationService.Navigate(new wConsultarProductos());
            }
        }

        private void BtnHomeClick(object sender, RoutedEventArgs e)
        {
            fmPrincipal.Navigate(new wPrincipalCliente());
        }
    }
}
