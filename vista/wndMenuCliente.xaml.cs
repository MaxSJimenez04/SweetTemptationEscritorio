using sweet_temptation_clienteEscritorio.model;
using sweet_temptation_clienteEscritorio.vista.cliente;
using sweet_temptation_clienteEscritorio.vista.pedido;
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

        private void BtnClickInicio(object sender, RoutedEventArgs e)
        {
            if (fmPrincipal.CanGoBack)
            {
                fmPrincipal.GoBack();
            }
            else
            {
                fmPrincipal.Navigate(new wPrincipalCliente());
            }
        }

        private void BtnClickCerrarSesion(object sender, RoutedEventArgs e)
        {
            App.Current.Properties.Clear();
            new wndLogin().Show();
            this.Close();
        }

        private void btnClickCarrito(object sender, RoutedEventArgs e)
        {
            Pedido pedido = new Pedido();
            pedido.id = 0;
            fmPrincipal.NavigationService.Navigate(new wPedido(pedido));
        }
    }
}
