using sweet_temptation_clienteEscritorio.dto;
using sweet_temptation_clienteEscritorio.servicios;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace sweet_temptation_clienteEscritorio.vista.pedidoPersonalizado {
    /// <summary>
    /// Lógica de interacción para wSolicitudesPersonalizadas.xaml
    /// </summary>
    public partial class wSolicitudesPersonalizadas : Page {

        public static readonly RoutedEvent ViewPedidoClickedEvent = EventManager.RegisterRoutedEvent(
            name: "ViewPedidoClicked",
            routingStrategy: RoutingStrategy.Bubble,
            handlerType: typeof(ViewPedidoClickedEventHandler), 
            ownerType: typeof(wSolicitudesPersonalizadas));

        public delegate void ViewPedidoClickedEventHandler(object sender, ViewPedidoClickedEventArgs e);

        public class ViewPedidoClickedEventArgs : RoutedEventArgs {
            public PedidoPersonalizadoDTO Pedido { get; set; }
            public ViewPedidoClickedEventArgs(RoutedEvent routedEvent, PedidoPersonalizadoDTO pedido) : base(routedEvent) {
                Pedido = pedido;
            }
        }

        public event ViewPedidoClickedEventHandler ViewPedidoClicked {
            add { AddHandler(ViewPedidoClickedEvent, value); }
            remove { RemoveHandler(ViewPedidoClickedEvent, value); }
        }

        private readonly PedidoPersonalizadoService _service;
        private List<PedidoPersonalizadoDTO> _todosLosPedidos = new();
        private int _paginaActual = 1;
        private const int _tamanoPagina = 5;

        public wSolicitudesPersonalizadas() {
            InitializeComponent();
            _service = new PedidoPersonalizadoService(new System.Net.Http.HttpClient());

            CargarSolicitudes(); 

            btnAnterior.Click += BtnAnterior_Click;
            btnSiguiente.Click += BtnSiguiente_Click;

            this.AddHandler(ViewPedidoClickedEvent, new ViewPedidoClickedEventHandler(OnViewPedidoClicked));
        }

        private void OnViewPedidoClicked(object sender, ViewPedidoClickedEventArgs e) {
            NavigationService.Navigate(
                new wDetallePedidoPersonalizado(e.Pedido)
            );

            lstPedidos.SelectedItem = null;
        }

        private async void CargarSolicitudes() {
            var pedidos = await _service.ObtenerSolicitudesAsync();

            if(pedidos == null) {
                MessageBox.Show("No se pudieron cargar las solicitudes. Verifique la conexión o autenticación.");
                _todosLosPedidos = new List<PedidoPersonalizadoDTO>();
                return;
            }

            _todosLosPedidos = pedidos
                .OrderByDescending(p => p.fechaSolicitud)
                .ToList();

            _paginaActual = 1;
            MostrarPagina();
        }

        private void MostrarPagina() {
            int skip = (_paginaActual - 1) * _tamanoPagina;

            lstPedidos.ItemsSource = _todosLosPedidos
                .Skip(skip)
                .Take(_tamanoPagina)
                .ToList();

            txtPagina.Text = $"Página {_paginaActual}";

            btnAnterior.IsEnabled = _paginaActual > 1;
            btnSiguiente.IsEnabled = _todosLosPedidos.Count > 0 && skip + _tamanoPagina < _todosLosPedidos.Count;
        }

        private void BtnSiguiente_Click(object sender, RoutedEventArgs e) {
            if((_paginaActual * _tamanoPagina) < _todosLosPedidos.Count) {
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
            if(NavigationService.CanGoBack)
                NavigationService.GoBack();
        }

        private void lstPedidos_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if(lstPedidos.SelectedItem is PedidoPersonalizadoDTO pedido) {

                NavigationService.Navigate(
                    new wDetallePedidoPersonalizado(pedido)
                );
                lstPedidos.SelectedItem = null;
            }
        }

        private void ListButton_Click(object sender, MouseButtonEventArgs e) {
            if(sender is ListBoxItem item) {

                if(item.DataContext is PedidoPersonalizadoDTO pedido) {
                    NavigationService.Navigate(
                        new wDetallePedidoPersonalizado(pedido)
                    );

                    if(lstPedidos.SelectedItem == pedido) {
                        lstPedidos.SelectedItem = null;
                    }
                }
            }
        }
    }
}