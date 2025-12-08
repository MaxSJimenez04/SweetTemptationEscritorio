using sweet_temptation_clienteEscritorio.dto;
using sweet_temptation_clienteEscritorio.servicios;
using sweet_temptation_clienteEscritorio.vista.cliente;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace sweet_temptation_clienteEscritorio.vista.pedido {
    /// <summary>
    /// Lógica de interacción para wHistorialPedidos.xaml
    /// </summary>
    public partial class wHistorialPedidos : Page {
        private readonly PedidoService _pedidoService;

        private List<PedidoDTO> _todosLosPedidos = new List<PedidoDTO>();

        private int _paginaActual = 1;
        private const int _tamanoPagina = 5;

        private int IdCliente {
            get {
                if(App.Current.Properties.Contains("Id") &&
                    App.Current.Properties["Id"] != null) {
                    return (int)App.Current.Properties["Id"];
                }

                MessageBox.Show("Error: No se encontró el IdCliente en la sesión.");
                return 0; 
            }
        }

        private string Token => App.Current.Properties["Token"]?.ToString();

        public wHistorialPedidos() {
            InitializeComponent();
            _pedidoService = new PedidoService(new System.Net.Http.HttpClient());

            CargarPedidos();
            btnAnterior.Click += BtnAnterior_Click;
            btnSiguiente.Click += BtnSiguiente_Click;
        }

        private async void CargarPedidos() {
            var (pedidos, codigo, mensaje) = await _pedidoService.ObtenerPedidosAsync(IdCliente, Token);

            if(pedidos == null) {
                MessageBox.Show("No se pudieron obtener los pedidos.\n" + mensaje);
                return;
            }

            _todosLosPedidos = pedidos
                .OrderByDescending(p => p.fechaCompra)
                .ToList();

            _paginaActual = 1;
            MostrarPagina();
        }

        private void MostrarPagina() {
            int skip = (_paginaActual - 1) * _tamanoPagina;

            var pedidosPagina = _todosLosPedidos
                .Skip(skip)
                .Take(_tamanoPagina)
                .ToList();

            lstPedidos.ItemsSource = pedidosPagina;

            txtPagina.Text = $"Página {_paginaActual}";

            btnAnterior.IsEnabled = _paginaActual > 1;
            btnSiguiente.IsEnabled = skip + _tamanoPagina < _todosLosPedidos.Count;
        }

        private void BtnSiguiente_Click(object sender, RoutedEventArgs e) {
            int maxPaginas = (int)Math.Ceiling((double)_todosLosPedidos.Count / _tamanoPagina);

            if(_paginaActual < maxPaginas) {
                _paginaActual++;
                MostrarPagina();
            }
        }

        private void BtnAnterior_Click(object sender, RoutedEventArgs e) {
            if(_paginaActual > 1) {
                _paginaActual--;
                MostrarPagina();
            }
        }

        private void btnClickRegresar(object sender, RoutedEventArgs e) {
            if(this.NavigationService != null) {
                this.NavigationService.Navigate(new wPrincipalCliente());
            }
        }
    }
}
