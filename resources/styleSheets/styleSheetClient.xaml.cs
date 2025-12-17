using sweet_temptation_clienteEscritorio.dto;
using sweet_temptation_clienteEscritorio.vista;
using sweet_temptation_clienteEscritorio.vista.pedido;
using sweet_temptation_clienteEscritorio.vista.pedidoPersonalizado;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;

namespace sweet_temptation_clienteEscritorio.resources.styleSheets {
    public partial class styleSheetClient : ResourceDictionary {
        public styleSheetClient() {
            InitializeComponent();
        }

        private void Menu_Click(object sender, MouseButtonEventArgs e) {
            if(sender is ListBoxItem item && item.Tag != null) {
                string accion = item.Tag.ToString();

                var ventanaActual = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
                Frame frame = ventanaActual?.FindName("fmPrincipal") as Frame;

                if(frame == null && ventanaActual != null && ventanaActual.Content is Frame f) 
                { 
                    frame = f;
                }


                if(frame != null || accion == "Logout") {
                    if(ventanaActual != null) {
                        switch(accion) {
                            case "Pedidos":
                                frame.Navigate(new wHistorialPedidos());
                                break;

                            case "Cuenta":
                                MessageBox.Show("Navegando a Mi Cuenta...");
                                break;

                            case "Logout":
                                new wndLogin().Show();
                                ventanaActual.Close();
                                break;
                        }
                    }
                }

                item.IsSelected = false;
            }
        }

        private static T FindParent<T>(DependencyObject child) where T : DependencyObject {
            DependencyObject parent = VisualTreeHelper.GetParent(child);
            while(parent != null && !(parent is T)) {
                parent = VisualTreeHelper.GetParent(parent);
            }
            return parent as T;
        }

        private void ListButton_PreviewMouseDown(object sender, MouseButtonEventArgs e) {

            if(sender is ListBoxItem item) {
                var originalSource = e.OriginalSource as DependencyObject;
                var button = FindParent<Button>(originalSource);

                if(button != null && button.Content.ToString() == "Ver") {

                    e.Handled = true;

                    if(item.DataContext is PedidoPersonalizadoDTO pedido) {

                        var ventanaActual = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
                        Frame frame = ventanaActual?.FindName("fmPrincipal") as Frame;

                        if(frame == null && ventanaActual != null && ventanaActual.Content is Frame f) {
                            frame = f;
                        }

                        if(frame != null) {
                            frame.Navigate(new wDetallePedidoPersonalizado(pedido));
                            if(item.Parent is ListBox listBox) {
                                listBox.SelectedItem = null;
                            }
                        } else {
                            MessageBox.Show("Error: No se encontró el Frame de navegación (fmPrincipal).");
                        }
                    }
                }
            }
        }


    }
}

