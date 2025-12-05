using sweet_temptation_clienteEscritorio.vista.empleado;
using System.Windows;

namespace sweet_temptation_clienteEscritorio.vista
{
    public partial class wndMenuEmpleado : Window
    {
        public wndMenuEmpleado()
        {
            InitializeComponent();
            fmPrincipal.Navigate(new wPrincipalEmpleado());
        }

        private void BtnClickRegresar(object sender, RoutedEventArgs e)
        {
            if (fmPrincipal.CanGoBack)
            {
                fmPrincipal.GoBack();
            }
            else
            {
                fmPrincipal.Navigate(new wPrincipalEmpleado());
            }
        }

        private void BtnClickCerrarSesion(object sender, RoutedEventArgs e)
        {
            App.Current.Properties.Clear();
            new wndLogin().Show();
            this.Close();
        }

        private void fmPrincipal_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {

        }
    }
}
