using sweet_temptation_clienteEscritorio.dto;
using sweet_temptation_clienteEscritorio.servicios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

// -------------------------------------------------------------
// CLASE MODELO WPF PARA EL DATAGRID (SIN NOMBRE CLIENTE)
// -------------------------------------------------------------
public class Venta
{
    // ❗ PROPIEDAD NombreCliente ELIMINADA ❗
    public string TipoPedido { get; set; }
    public DateTime FechaCompra { get; set; }
    public string Estado { get; set; }
    public decimal Total { get; set; }
}

// -------------------------------------------------------------
// INICIO DEL CODE-BEHIND
// -------------------------------------------------------------
namespace sweet_temptation_clienteEscritorio.vista.Estadisticas
{
    public partial class wEstadisticasVentas : Page
    {
        private PedidoService _servicioPedido;
        private string _token;

        // Diccionario para traducir IDs de Estado de la API a texto para la UI
        private readonly Dictionary<int, string> _estadosApiMap = new Dictionary<int, string>
        {
            { 3, "Completada" }, // Asumido 3 en la API
            { 4, "Cancelada" },  // Asumido 4 en la API
            { 2, "Pendiente" }  // Asumido 2 en la API
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

        // ===================================================
        // MANEJADORES DE EVENTOS
        // ===================================================

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

        // ===================================================
        // LÓGICA DE CONEXIÓN Y CONSULTA REAL
        // ===================================================

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

                // --------------------------------------------------
                // ❗ 1. VALIDACIONES DE FECHA ❗
                // --------------------------------------------------

                // Usamos DateTime.Today para comparaciones, ignorando la hora
                DateTime hoy = DateTime.Today;

                // Capturar fechas seleccionadas (Si es nulo, usamos valores por defecto para no romper el flujo)
                DateTime? fechaInicialSeleccionada = dpFechaInicial.SelectedDate;
                DateTime? fechaFinalSeleccionada = dpFechaFinal.SelectedDate;

                // Si ambas están seleccionadas, validamos el orden lógico.
                if (fechaInicialSeleccionada.HasValue && fechaFinalSeleccionada.HasValue)
                {
                    if (fechaInicialSeleccionada.Value > fechaFinalSeleccionada.Value)
                    {
                        MessageBox.Show("La Fecha Inicial no puede ser posterior a la Fecha Final.", "Error de Filtro", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return; // Detiene la ejecución
                    }
                }

                // Validar que la Fecha Final no sea una fecha futura
                if (fechaFinalSeleccionada.HasValue && fechaFinalSeleccionada.Value > hoy)
                {
                    MessageBox.Show("No se pueden consultar ventas de fechas futuras.", "Error de Filtro", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return; // Detiene la ejecución
                }

                // --------------------------------------------------
                // 2. Obtener y preparar filtros
                // Si el DatePicker es nulo, usamos valores amplios/seguros para la API.
                DateTime fechaInicial = fechaInicialSeleccionada ?? DateTime.Today.AddYears(-1);
                DateTime fechaFinal = fechaFinalSeleccionada ?? DateTime.Today;

                string estadoFiltroTexto = (cbEstadoVenta.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Todas";

                try
                {
                    // 3. Obtener datos reales de la API
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

                    // 4. Transformar PedidoDTO a Venta
                    var ventasParaMostrar = pedidosDTO.Select(p => new Venta
                    {
                        // Mapeo
                        TipoPedido = p.personalizado ? "Personalizado" : "Vitrina",
                        Estado = _estadosApiMap.GetValueOrDefault(p.estado, "Desconocido"),

                        FechaCompra = p.fechaCompra,
                        Total = p.total
                    }).ToList();

                    // 5. Asignar al DataGrid
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