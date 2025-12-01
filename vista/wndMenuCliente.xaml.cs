using sweet_temptation_clienteEscritorio.vista.cliente;
using System.Windows;

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
    }
}
