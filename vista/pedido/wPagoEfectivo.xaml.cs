using sweet_temptation_clienteEscritorio.dto;
using sweet_temptation_clienteEscritorio.servicios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
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
    /// Lógica de interacción para wPagoEfectivo.xaml
    /// </summary>
    public partial class wPagoEfectivo : Page {
        private decimal total;
        private int idPedido;

        private readonly PagoService pagoService;

        public wPagoEfectivo() {
            InitializeComponent();
            pagoService = new PagoService(new HttpClient());
            CargarDatos();
        }

        private void CargarDatos() {
            var pedido = (PedidoDTO)App.Current.Properties["PedidoActual"];
            total = pedido.total;
            idPedido = pedido.id;
        }

        private async void BtnClickPagar(object sender, RoutedEventArgs e) {
            if(!decimal.TryParse(TxtNumero.Text, out decimal cantidadIngresada)) {
                MessageBox.Show("Ingrese una cantidad válida.", "Advertencia",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            decimal cambio = cantidadIngresada - total;

            if(cambio < 0) {
                MessageBox.Show("La cantidad ingresada es insuficiente.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            txtCambio.Text = $"Cambio: ${cambio:0.00}";

            MessageBox.Show(
                $"Efectivo: ${cantidadIngresada:0.00}\nCambio: ${cambio:0.00}",
                "Pago en efectivo",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            string token = App.Current.Properties["Token"]?.ToString();

            var request = new PagoRequestDTO {
                TipoPago = "Efectivo",
                MontoPagado = cantidadIngresada,
            };

            var (pago, codigo, mensaje) =
                await pagoService.RegistrarPagoAsync(idPedido, request, token);

            if(codigo == HttpStatusCode.OK || codigo == HttpStatusCode.Created) {
                MessageBox.Show("Pago exitoso", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                NavigationService?.GoBack();
            } else {
                MessageBox.Show($"Error al registrar el pago:\n{mensaje}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}