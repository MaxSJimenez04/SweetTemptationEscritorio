using sweet_temptation_clienteEscritorio.dto;
using sweet_temptation_clienteEscritorio.servicios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

public class Venta
{
    public string TipoPedido { get; set; }
    public DateTime FechaCompra { get; set; }
    public string Estado { get; set; }
    public decimal Total { get; set; }
}

namespace sweet_temptation_clienteEscritorio.vista.Estadisticas
{
    public partial class wEstadisticasVentas : Page
    {
        private PedidoService _servicioPedido;
        private string _token;

        // TODO verificar estados
        private readonly Dictionary<int, string> _estadosApiMap = new Dictionary<int, string>
        {
            { 3, "Completada" }, 
            { 4, "Cancelada" },  
            { 2, "Pendiente" }  
        };

        public wEstadisticasVentas()
        {
            InitializeComponent();

            var http = new HttpClient();
            _servicioPedido = new PedidoService(http);

            if (App.Current.Properties.Contains("Token"))
                _token = (string)App.Current.Properties["Token"];

            Loaded += async (s, e) =>
            {
                await CargarVentas();
            };
        }


        private async void btnBuscar_Click(object sender, RoutedEventArgs e)
        {
            await CargarVentas();
        }

        private void btnDescargar_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Funcionalidad CSV en desarrollo...", "Descarga", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void BtnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            dpFechaInicial.SelectedDate = null;
            dpFechaFinal.SelectedDate = null;
            cbEstadoVenta.SelectedIndex = 0;
            await CargarVentas();
        }


        private async Task CargarVentas()
        {
            await Application.Current.Dispatcher.Invoke(async () =>
            {
                if (string.IsNullOrEmpty(_token))
                {
                    MessageBox.Show("No se ha detectado una sesión activa para cargar ventas.", "Error de Autenticación", MessageBoxButton.OK, MessageBoxImage.Error);
                    dgVentas.ItemsSource = null;
                    return;
                }

                DateTime hoy = DateTime.Today;

                DateTime? fechaInicialSeleccionada = dpFechaInicial.SelectedDate;
                DateTime? fechaFinalSeleccionada = dpFechaFinal.SelectedDate;

                if (fechaInicialSeleccionada.HasValue && fechaFinalSeleccionada.HasValue)
                {
                    if (fechaInicialSeleccionada.Value > fechaFinalSeleccionada.Value)
                    {
                        MessageBox.Show("La Fecha Inicial no puede ser posterior a la Fecha Final.", "Error de Filtro", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return; 
                    }
                }

                if (fechaFinalSeleccionada.HasValue && fechaFinalSeleccionada.Value > hoy)
                {
                    MessageBox.Show("No se pueden consultar ventas de fechas futuras.", "Error de Filtro", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return; 
                }

                DateTime fechaInicial = fechaInicialSeleccionada ?? DateTime.Today.AddYears(-1);
                DateTime fechaFinal = fechaFinalSeleccionada ?? DateTime.Today;

                string estadoFiltroTexto = (cbEstadoVenta.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Todas";

                try
                {
                    var (pedidosDTO, codigo, mensaje) = await _servicioPedido.ConsultarVentasAsync(
                        fechaInicial, fechaFinal, estadoFiltroTexto, _token
                    );

                    if (pedidosDTO == null || codigo != System.Net.HttpStatusCode.OK && codigo != System.Net.HttpStatusCode.NoContent)
                    {
                        dgVentas.ItemsSource = null;
                        MessageBox.Show($"Error al cargar ventas (HTTP {codigo}): {mensaje}", "Error de API", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (pedidosDTO.Count == 0)
                    {
                        dgVentas.ItemsSource = null;
                        MessageBox.Show("No se encontraron ventas con los filtros seleccionados.", "Sin Resultados", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var ventasParaMostrar = pedidosDTO.Select(p => new Venta
                    {
                        TipoPedido = p.personalizado ? "Personalizado" : "Vitrina",
                        Estado = _estadosApiMap.GetValueOrDefault(p.estado, "Desconocido"),

                        FechaCompra = p.fechaCompra,
                        Total = p.total
                    }).ToList();

                    dgVentas.ItemsSource = ventasParaMostrar;
                }
                catch (Exception ex)
                {
                    dgVentas.ItemsSource = null;
                    MessageBox.Show($"Error fatal al cargar ventas: {ex.Message}", "Error Inesperado", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });
        }

        private void BtnRegresarClick(object sender, RoutedEventArgs e)
        {
            if(NavigationService.CanGoBack)
                NavigationService.GoBack();
        }
    }
}