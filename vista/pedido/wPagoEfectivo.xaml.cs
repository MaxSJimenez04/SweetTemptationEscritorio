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
using sweet_temptation_clienteEscritorio.model;

namespace sweet_temptation_clienteEscritorio.vista.pedido
{
    /// <summary>
    /// Lógica de interacción para wPagoEfectivo.xaml
    /// </summary>
    public partial class wPagoEfectivo : Page {
        private decimal total;
        private int idPedido;
        private Pedido _pedido;

        private readonly PagoService pagoService;
        private readonly ProductoPedidoService productoPedidoService;
        private TicketGrpcService ticketService;

        public wPagoEfectivo(Pedido pedido) {
            InitializeComponent();
            _pedido = pedido;
            pagoService = new PagoService(new HttpClient());
            productoPedidoService = new ProductoPedidoService(new HttpClient());
            ticketService = new TicketGrpcService();
            CargarDatos();
        }

        private void CargarDatos() {
            total = _pedido.total;
            idPedido = _pedido.id;
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

            List<DetallesProductoDTO> productosComprados;
            var respuestaProductos = await productoPedidoService.obtenerProductosAsync(idPedido, token);
            if (respuestaProductos.productos != null && respuestaProductos.codigo == HttpStatusCode.OK)
            {
                productosComprados = respuestaProductos.productos;
                var respuesta = await productoPedidoService.comprarProductosAsync(idPedido, productosComprados, token);
                if (respuesta.codigo == HttpStatusCode.OK)
                {
                    var (pago, codigo, mensaje) =
                    await pagoService.RegistrarPagoAsync(idPedido, request, token);

                    if (codigo == HttpStatusCode.OK || codigo == HttpStatusCode.Created)
                    {
                        MessageBox.Show("Pago exitoso", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                        await GenerarTicketAsync();
                        NavigationService?.GoBack();
                    }
                    else
                    {
                        MessageBox.Show($"Error al registrar el pago:\n{mensaje}", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Uno o varios productos han agotado sus existencias", "Cantidad insuficiente", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("No se han seleccionado productos", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
        }

        private async Task GenerarTicketAsync()
        {
            var respuesta = await ticketService.DescargarTicketAsync(_pedido.id);

            if (respuesta != null)
            {
                MessageBox.Show("Ticket descargado en" + respuesta);
            }
            else
            {
                MessageBox.Show("ERROR: ocurrió un problema al generar el ticket");
            }
        }
    }
}