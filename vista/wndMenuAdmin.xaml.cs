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
    /// Lógica de interacción para wndMenuAdmin.xaml
    /// </summary>
    public partial class wndMenuAdmin : Window
    {
        public wndMenuAdmin()
        {
            InitializeComponent();
        }

        private void BtnClickRegresar(object sender, RoutedEventArgs e)
        {
            if (fmPrincipal.CanGoBack)
            {
                fmPrincipal.GoBack();
            }
        }
    }
}
