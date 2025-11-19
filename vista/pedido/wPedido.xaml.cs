using sweet_temptation_clienteEscritorio.dto;
using sweet_temptation_clienteEscritorio.model;
using sweet_temptation_clienteEscritorio.resources;
using sweet_temptation_clienteEscritorio.resources.usercontrolers;
using sweet_temptation_clienteEscritorio.servicios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
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

    public partial class wPedido : Page
    {
        private PedidoService _servicioPedido;
        private ProductoPedidoService _servicioProductoPedido;
        private Pedido _pedido;
        private int _idUsuario = 3;
        private List<DetallesProducto> _detallesProductos;

        public wPedido(Pedido pedido)
        {
            InitializeComponent();
            _servicioPedido = new PedidoService(new HttpClient());
            _servicioProductoPedido = new ProductoPedidoService(new HttpClient());
            _pedido = pedido;
            _detallesProductos = new List<DetallesProducto>();
            if(pedido.id == 0)
            {
                Loaded += async (s, e) =>
                {
                    await ObtenerPedidoActualAsync();
                    await ObtenerProductosAsync();
                };
            }
            else
            {
                Loaded += async (s, e) =>
                {
                    await ObtenerProductosAsync();
                };
            }

        }
        public async void CalcularTotal()
        {
            Decimal resultado = 0;
            foreach (var item in _detallesProductos)
            {
                resultado += item.subtotal;
            }

            lbSubtotal.Content += $"{resultado}";
            Decimal total = resultado + Constantes.IVA;
            await CambiarTotalPedidoAsync(total);
            LlenarDatosPedido();
        }

        public void LlenarDatosPedido()
        {
            lbTotal.Content += $"{_pedido.total}";
            lbImpuesto.Content += $"{Constantes.IVA}%";
        }

        public void VaciarDatosPedido()
        {
            lbTotal.Content = "Total: $";
            lbImpuesto.Content = "IVA: ";
            lbSubtotal.Content = "Subtotal: $";
        }

        public async void LlenarProductos()
        {
            wpProductos.Children.Clear();
            foreach (var item in _detallesProductos)
            {
                ucDetallesProducto userControl = new ucDetallesProducto(item);
                wpProductos.Children.Add(userControl);
                userControl.OnEliminar += async (detalles) =>
                {
                    await EliminarProductoAsync(detalles.id);
                    _detallesProductos.Remove(detalles);
                    wpProductos.Children.Remove(userControl);
                    VaciarDatosPedido();
                };
            }
        }

        private async void btnClickCancelar(object sender, RoutedEventArgs e)
        {
            await CancelarPedidoAsync();
            await CrearPedidoClienteAsync();
            await ObtenerProductosAsync();
        }

        private void btnClickRealizar(object sender, RoutedEventArgs e)
        {
            //TODO: Navegar a WPago
        }

        private void btnClickEditar(object sender, RoutedEventArgs e)
        {
            btnEditar.Visibility = Visibility.Collapsed;
            btnGuardar.Visibility = Visibility.Visible;
            VaciarDatosPedido();
            foreach (var userControl in wpProductos.Children)
            {
                if (userControl is ucDetallesProducto uc)
                {
                    uc.HabilitarEdicion();
                }
            }
        }

        private async void BtnClickGuardar(object sender, RoutedEventArgs e)
        {
            btnGuardar.Visibility = Visibility.Collapsed;
            btnEditar.Visibility = Visibility.Visible;
            foreach (var userControl in wpProductos.Children)
            {
                if (userControl is ucDetallesProducto uc)
                {
                    await ActualizarProductosAsync(uc.detalles);
                    uc.DeshabilitarEdicion();

                }
            }
            CalcularTotal();
        }

        private void BtnClickProductos(object sender, RoutedEventArgs e)
        {
            //TODO: Navegar a WProductos
        }

        public async Task ObtenerPedidoActualAsync()
        {
            var respuesta = await _servicioPedido.ObtenerPedidoActualAsync(_idUsuario);
            if (respuesta.pedidoActual == null)
            {
                MessageBox.Show(respuesta.mensaje);
            }
            else
            {
                _pedido.id = respuesta.pedidoActual.id;
                _pedido.actual = respuesta.pedidoActual.actual;
                _pedido.personalizado = respuesta.pedidoActual.personalizado;
                _pedido.estado = respuesta.pedidoActual.estado;
                _pedido.total = respuesta.pedidoActual.total;
                _pedido.fechaCompra = respuesta.pedidoActual.fechaCompra;
                _pedido.idCliente = respuesta.pedidoActual.idCliente;
            }
        }

        public async Task CancelarPedidoAsync()
        {
            var respuesta = await _servicioPedido.CancelarPedidoAsync(_pedido.id, _idUsuario);

            switch (respuesta.codigo)
            {
                case HttpStatusCode.OK:
                    VaciarDatosPedido();
                    wpProductos.Children.Clear();
                    MessageBox.Show("Pedido cancelado con éxito");
                    break;
                case HttpStatusCode.BadRequest:
                    MessageBox.Show("ERROR: ID no válido");
                    break;
                case HttpStatusCode.NotFound:
                    MessageBox.Show("ERROR: No se encontró el pedido actual");
                    break;
            }
        }

        public async Task CrearPedidoClienteAsync()
        {
            var respuesta = await _servicioPedido.CrearPedidoClienteAsync(_idUsuario);
            if (respuesta)
            {
                await ObtenerPedidoActualAsync();
            }
            else
            {
                MessageBox.Show("ERROR: Ocurrió un error al cancelar el pedido");
            }
        }

        public async Task ObtenerProductosAsync()
        {
            var respuesta = await _servicioProductoPedido.obtenerProductosAsync(_pedido.id);
            if (respuesta.productos != null)
            {
                spError.Visibility = Visibility.Collapsed;
                svProductos.Visibility = Visibility.Visible;
                btnEditar.Visibility = Visibility.Visible;
                foreach (var producto in respuesta.productos)
                {
                    _detallesProductos.Add(new()
                    {
                       id = producto.id,
                       cantidad = producto.cantidad,
                       nombre = producto.nombre,
                       precio = producto.precio,
                       subtotal = producto.subtotal,
                       idProducto = producto.idProducto,
                    });
                }

                LlenarProductos();
                CalcularTotal();
            }
            else
            {
                btnEditar.Visibility = Visibility.Collapsed;
                spError.Visibility = Visibility.Visible;
                btnCancelar.Visibility = Visibility.Collapsed;
                btnRealizarPedido.Visibility = Visibility.Collapsed;
                svProductos.Visibility = Visibility.Collapsed;
                wpProductos.Visibility = Visibility.Collapsed;
                
            }
        }

        public async Task CambiarTotalPedidoAsync(Decimal total)
        {
            var respuesta = await _servicioPedido.CambiarTotalPedidoAsync(_pedido.id, total);

            if (respuesta.pedidoActualizado != null)
            {
                _pedido.total = respuesta.pedidoActualizado.total;
            }
            else
            {
                MessageBox.Show("ERROR: " + respuesta.mensaje);
            }
        }

        public async Task ActualizarProductosAsync(DetallesProducto detalles)
        {
            ProductoPedidoDTO pedido = new()
            {
                Id = detalles.id,
                Cantidad = detalles.cantidad,
                Subtotal = detalles.subtotal,
                IdPedido = _pedido.id,
                IdProducto = detalles.idProducto
            };
            var respuesta = await _servicioProductoPedido.actualizarProductoAsync(_pedido.id, pedido);
            if(respuesta.productoActualizado == null)
            {
                MessageBox.Show(respuesta.mensaje);
            }
        }
        
        public async Task EliminarProductoAsync(int idProducto)
        {
            var respuesta = await _servicioProductoPedido.eliminarProductoAsync(_pedido.id, idProducto);
            switch (respuesta.codigo)
            {
                case HttpStatusCode.BadRequest:
                    MessageBox.Show(respuesta.mensaje);
                    break;
                case HttpStatusCode.InternalServerError:
                    MessageBox.Show(respuesta.mensaje);
                    break;
            }
        }
    }
}
