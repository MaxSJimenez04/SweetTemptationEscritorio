using sweet_temptation_clienteEscritorio.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace sweet_temptation_clienteEscritorio.vista.pedido
{
    /// <summary>
    /// Lógica de interacción para wTipoPago.xaml
    /// </summary>
    public partial class wTipoPago : Page
    {
        private Pedido _pedidoPago;
        public wTipoPago(Pedido pedidoAPagar)
        {
            InitializeComponent();
            _pedidoPago = pedidoAPagar;
        }

        private void btnClickEfectivo(object sender, RoutedEventArgs e) {
            if(this.NavigationService != null) {
                this.NavigationService.Navigate(new wPagoEfectivo(_pedidoPago));
            }
        }

        private void btnClickTarjeta(object sender, RoutedEventArgs e) {
            if(this.NavigationService != null) {
                this.NavigationService.Navigate(new wPagoTarjeta(_pedidoPago));
            }
        }

        private void btnClickRegresar(object sender, RoutedEventArgs e) {
            if(NavigationService.CanGoBack)
                NavigationService.GoBack();
        }

    }
}
