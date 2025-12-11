using sweet_temptation_clienteEscritorio.vista.Estadisticas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
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

namespace sweet_temptation_clienteEscritorio.vista.admin
{
    public partial class wSeleccionEstadisticas : Page
    {
        public wSeleccionEstadisticas()
        {
            InitializeComponent();
        }

        private void BtnRegresarClick(object sender, RoutedEventArgs e)
        {
           if(NavigationService.CanGoBack) 
                NavigationService.GoBack();
        }

        private void BtnVentasClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new wEstadisticasVentas());
        }

        private void BtnProductosClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new wEstadisticasProductos());
        }
    }
}
