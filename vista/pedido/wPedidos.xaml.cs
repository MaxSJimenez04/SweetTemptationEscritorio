using sweet_temptation_clienteEscritorio.model;
using sweet_temptation_clienteEscritorio.resources.usercontrolers;
using sweet_temptation_clienteEscritorio.servicios;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public partial class wPedidos : Page
    {
        private int _idUsuario;
        private PedidoService _servicio;
        private ProductoPedidoService _productoPedidoService;
        public ObservableCollection<Pedido> _pedidos;
        string _token;
        public wPedidos()
        {
            _idUsuario = (int)App.Current.Properties["Id"];
            _servicio = new PedidoService(new HttpClient());
            _pedidos = new ObservableCollection<Pedido>();
            _token = (string?)App.Current.Properties["Token"];
            _productoPedidoService = new ProductoPedidoService(new HttpClient());

            InitializeComponent();
            Loaded += async (s, e) =>
            {
                await ObtenerPedidosAsync();

            };
        }

        private void LlenarPedidos()
        {
            icPedidos.Items.Clear();
            foreach (var pedido in _pedidos)
            {
                var ucPedido = new ucPedido(pedido);
                ucPedido.Eliminar += async (s, pedido) =>
                {
                    await EliminarPedidoAsync(pedido);
                };
                ucPedido.Modificar += async (pedido) =>
                {
                    this.NavigationService.Navigate(new wPedido(pedido));
                };
                icPedidos.Items.Add(ucPedido);
            }
        }

        private async void BtnClickNuevo(object sender, RoutedEventArgs e)
        {
            await CrearPedidoAsync();
        }

        private async Task ObtenerPedidosAsync()
        {
            _pedidos.Clear();
            var respuesta = await _servicio.ObtenerPedidosAsync(_idUsuario, _token);
            if (respuesta.pedidos != null)
            {
                if (respuesta.pedidos.Count > 0)
                {
                    foreach (var pedido in respuesta.pedidos)
                    {
                        Pedido pedidoNuevo = new Pedido()
                        {
                            id = pedido.id,
                            actual = pedido.actual,
                            estado = pedido.estado,
                            fechaCompra = pedido.fechaCompra,
                            idCliente = pedido.idCliente,
                            personalizado = pedido.personalizado,
                            total = pedido.total
                        };
                        _pedidos.Add(pedidoNuevo);
                    }

                    LlenarPedidos();
                }   
            }
            else
            {
                MessageBox.Show("No se encontraron pedidos", "Pedidos", MessageBoxButton.OK);
            }
        }

        public async Task CrearPedidoAsync()
        {
            var respuesta = await _servicio.CrearPedidoEmpleadoAsync(_idUsuario, _token);

            if (respuesta.pedido != null)
            {
                Pedido pedidoNuevo = new Pedido()
                {
                    id = respuesta.pedido.id,
                    actual= respuesta.pedido.actual,
                    estado = respuesta.pedido.estado,
                    fechaCompra = respuesta.pedido.fechaCompra,
                    personalizado = respuesta.pedido.personalizado,
                    total= respuesta.pedido.total,
                    idCliente = respuesta.pedido.idCliente
                };
                _pedidos.Add(pedidoNuevo);

                this.NavigationService.Navigate(new wPedido(pedidoNuevo));
            }
            else
            {
                MessageBox.Show("Ocurrió un error al crear un pedido", "Crear pedido", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task EliminarPedidoAsync(Pedido pedido)
        {
            var detallesProductosPedido = await _productoPedidoService.obtenerProductosAsync(pedido.id, _token);
            if (detallesProductosPedido.codigo == HttpStatusCode.OK && detallesProductosPedido.productos != null)
            {
                foreach (var item in detallesProductosPedido.productos)
                {
                   await _productoPedidoService.eliminarProductoAsync(pedido.id, item.id, _token);
                }
            }

            var respuesta = await _servicio.EliminarPedidoAsync(pedido.id, _token);
            if (respuesta.mensaje != null)
            {
                MessageBox.Show("Ocurrió un error al eliminar el pedido", "Eliminar pedido", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                var ucPedido = icPedidos.Items.OfType<ucPedido>().FirstOrDefault(x => x.id == pedido.id);
                if (ucPedido != null)
                {
                    icPedidos.Items.Remove(ucPedido);
                }
            }
        }

        private void BtnRegresarClick(object sender, RoutedEventArgs e)
        {
            if(NavigationService.CanGoBack)
                NavigationService.GoBack();
        }
    }
}
