using sweet_temptation_clienteEscritorio.vista.pedido;
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
using System.Windows.Shapes;

namespace sweet_temptation_clienteEscritorio.vista
{
    /// <summary>
    /// Lógica de interacción para wndMenuCliente.xaml
    /// </summary>
    public partial class wndMenuCliente : Window
    {
        public wndMenuCliente()
        {
            InitializeComponent();
            
        }

        private void btnClickPedido(object sender, RoutedEventArgs e)
        {
            fmPrincipal.Navigate(new wPedido());
        }

        private void btnClickRegresar(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
