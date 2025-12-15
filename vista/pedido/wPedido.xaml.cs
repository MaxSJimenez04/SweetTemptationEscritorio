using sweet_temptation_clienteEscritorio.dto;
using sweet_temptation_clienteEscritorio.model;
using sweet_temptation_clienteEscritorio.resources;
using sweet_temptation_clienteEscritorio.resources.usercontrolers;
using sweet_temptation_clienteEscritorio.servicios;
using sweet_temptation_clienteEscritorio.vista.producto;
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
        private readonly string _rolUsuario;
        private ArchivoService _archivoService;
        private Pedido _pedido;
        private int _idUsuario;
        private List<DetallesProducto> _detallesProductos;
        string _token;
        public wPedido(Pedido pedido)
        {
            InitializeComponent();
            _servicioPedido = new PedidoService(new HttpClient());
            _servicioProductoPedido = new ProductoPedidoService(new HttpClient());
            _archivoService = new ArchivoService(new HttpClient());
            if (App.Current.Properties.Contains("Id"))
            {
                if (App.Current.Properties["Id"] is int id)
                {
                    _idUsuario = id;
                }
                else if (App.Current.Properties["Id"] is string idString && int.TryParse(idString, out int idConverted))
                {
                    _idUsuario = idConverted;
                }
                else
                {
                    MessageBox.Show("Error de sesión: El formato de la ID de usuario no es válido.");
                    _idUsuario = -1;
                }
            }
            else
            {
                MessageBox.Show("Error de sesión: No se encontró la ID de usuario.");
                _idUsuario = -1;
            }

            if (App.Current.Properties.Contains("Rol"))
            {
                _rolUsuario = (string?)App.Current.Properties["Rol"];
            }
            else
            {
                _rolUsuario = "Cliente";
            }
            _token = (string?)App.Current.Properties["Token"];
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
                string detalles = await ObtenerDetallesAsync(item.idProducto);
                BitmapImage img = await ObtenerImagenAsync(detalles);
                userControl.ColocarImagen(img);
                
            }
        }

        private async void btnClickCancelar(object sender, RoutedEventArgs e)
        {
            await CancelarPedidoAsync();
            switch (_rolUsuario)
            {
                case "Empleado":
                    break;
                case "Cliente":
                    await CrearPedidoClienteAsync();
                    await ObtenerProductosAsync();
                    break;
            }

           
        }
        private async void btnClickRealizar(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new wTipoPago(_pedido));
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
            NavigationService.Navigate(new wConsultarProductos(_pedido.id));
        }

        public async Task ObtenerPedidoActualAsync()
        {
            var respuesta = await _servicioPedido.ObtenerPedidoActualAsync(_idUsuario, _token);
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
            var respuesta = await _servicioPedido.CancelarPedidoAsync(_pedido.id, _idUsuario, _token);

            switch (respuesta.codigo)
            {
                case HttpStatusCode.OK:
                    VaciarDatosPedido();
                    wpProductos.Children.Clear();
                    MessageBox.Show("Pedido cancelado con éxito", "Cancelar Pedido", MessageBoxButton.OK);
                    break;
                case HttpStatusCode.BadRequest:
                    MessageBox.Show("ERROR: ID del pedido inválida", "Cancelar Pedido", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                case HttpStatusCode.NotFound:
                    MessageBox.Show("ERROR: No se encontró el pedido actual", "Cancelar Pedido", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                case HttpStatusCode.InternalServerError:
                    MessageBox.Show("Ocurrió un error al cancelar el pedido. Por favor intentelo más tarde", "Cancelar Pedido", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;

            }
        }

        public async Task CrearPedidoClienteAsync()
        {
            var respuesta = await _servicioPedido.CrearPedidoClienteAsync(_idUsuario, _token);
            if (respuesta)
            {
                await ObtenerPedidoActualAsync();
            }
            else
            {
                MessageBox.Show("ERROR: Ocurrió un error al crear un nuevo pedido", "Cancelar Pedido", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public async Task ObtenerProductosAsync()
        {
            var respuesta = await _servicioProductoPedido.obtenerProductosAsync(_pedido.id, _token);
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
            var respuesta = await _servicioPedido.CambiarTotalPedidoAsync(_pedido.id, total, _token);

            if (respuesta.pedidoActualizado != null)
            {
                _pedido.total = respuesta.pedidoActualizado.total;
            }
            else
            {
                MessageBox.Show("Ocurrió un error al calcular el total del producto", "Pedido", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            var respuesta = await _servicioProductoPedido.actualizarProductoAsync(_pedido.id, pedido, _token);
            if(respuesta.codigo != HttpStatusCode.OK && respuesta.mensaje != null)
            {
                MessageBox.Show("Error al modificar la cantidad del producto", "Pedido", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        public async Task EliminarProductoAsync(int idProducto)
        {
            var respuesta = await _servicioProductoPedido.eliminarProductoAsync(_pedido.id, idProducto, _token);
            switch (respuesta.codigo)
            {
                case HttpStatusCode.BadRequest:
                    MessageBox.Show("El producto ya se ha eliminado", "Eliminar Producto", MessageBoxButton.OK, MessageBoxImage.Hand);
                    break;
                case HttpStatusCode.InternalServerError:
                    MessageBox.Show("Ocurrió un error al eliminar el producto seleccionado, por favor inténtelo más tarde","Eliminar producto", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
            }
        }

        public async Task<string> ObtenerDetallesAsync(int idProducto)
        {
            var respuesta = await _archivoService.ObtenerDetallesArchivoAsync(idProducto, _token);
            DetallesArchivo detalles = new DetallesArchivo();
            if (respuesta.detalles != null)
            {
                detalles.ruta = respuesta.detalles.ruta;
                return detalles.ruta;
            }
            else
            {
                MessageBox.Show("No se pudo obtener la ruta del producto", "Recuperar producto", MessageBoxButton.OK, MessageBoxImage.Warning);
                return detalles.ruta;
            }
            
        }

        public async Task<BitmapImage> ObtenerImagenAsync(string ruta)
        {
            var respuesta = await _archivoService.ObtenerImagenAsync(ruta, _token);
            if(respuesta.imagen != null)
            {
                return respuesta.imagen;
            }
            else
            {
                MessageBox.Show("No se pudo obtener la imagen del producto", "Detalles del producto", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        private void BtnRegresarClick(object sender, RoutedEventArgs e)
        {
            if(NavigationService.CanGoBack)
                NavigationService.GoBack();
        }
    }
}
