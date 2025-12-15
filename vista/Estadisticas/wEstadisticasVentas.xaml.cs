using Microsoft.Win32;
using sweet_temptation_clienteEscritorio.dto;
using sweet_temptation_clienteEscritorio.resources;
using sweet_temptation_clienteEscritorio.servicios;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

public class Venta
{
    public string TipoPedido { get; set; }
    public DateTime FechaCompra { get; set; }
    public string Estado { get; set; }
    public decimal Total { get; set; }
    public string NombreRol { get; set; }
}

namespace sweet_temptation_clienteEscritorio.vista.Estadisticas
{
    public partial class wEstadisticasVentas : Page
    {
        private EstadisticasService _servicioEstadisticas;
        private string _token;

        // TODO verificar estados
        private readonly Dictionary<int, string> _estadosApiMap = new Dictionary<int, string>
        {
            { 3, "Completada" }, 
            { 4, "Cancelada" },  
            { 2, "Pendiente" }  
        };

        private readonly Dictionary<int, string> _rolMap = new Dictionary<int, string>
        {
            { 0, "Error Rol" }, 
            { 1, "Administrador" },
            { 2, "Empleado" },
            { 3, "Cliente" }
        };

        public wEstadisticasVentas()
        {
            InitializeComponent();

            var http = new HttpClient();
            _servicioEstadisticas = new EstadisticasService(http);

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

        private async void btnDescargar_Click(object sender, RoutedEventArgs e)
        {
            // Usamos el formato ISO DATE (yyyy-MM-dd) que Java espera. Si no hay fecha, enviamos string.Empty.
            string fechaInicio = dpFechaInicial.SelectedDate.HasValue
                ? dpFechaInicial.SelectedDate.Value.ToString("yyyy-MM-dd")
                : string.Empty; // Envía vacío si no hay fecha

            string fechaFin = dpFechaFinal.SelectedDate.HasValue
                ? dpFechaFinal.SelectedDate.Value.ToString("yyyy-MM-dd")
                : string.Empty; // Envía vacío si no hay fecha

            // Obtener el estado seleccionado
            string estado = (cbEstadoVenta.SelectedItem as ComboBoxItem)?.Content.ToString();

            if (estado == "Todas" || string.IsNullOrEmpty(estado))
            {
                estado = string.Empty;
            }

            string ip = Constantes.IP;
            string puerto = Constantes.PUERTO;

            string baseUrl = $"http://{ip}:{puerto}/estadisticas/ventas/descargarCSV";

            var queryParams = new List<string>
            {
                $"fechaInicio={fechaInicio}",
                $"fechaFin={fechaFin}",
                $"estado={estado}" 
            };

            string urlCompleta = baseUrl + "?" + string.Join("&", queryParams);

            try
            {
                // Realizar la solicitud HTTP con HttpClient
                using (var client = new HttpClient())
                {
                    if (!string.IsNullOrEmpty(_token))
                    {
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
                    }

                    HttpResponseMessage response = await client.GetAsync(urlCompleta);

                    if (response.IsSuccessStatusCode)
                    {
                        // Leer el contenido binario (para el archivo CSV)
                        byte[] fileBytes = await response.Content.ReadAsByteArrayAsync();

                        string defaultFileName = "reporte_ventas.csv";
                        if (response.Content.Headers.ContentDisposition != null &&
                            !string.IsNullOrEmpty(response.Content.Headers.ContentDisposition.FileName))
                        {
                            defaultFileName = response.Content.Headers.ContentDisposition.FileName.Trim('"');
                        }

                        SaveFileDialog saveDialog = new SaveFileDialog
                        {
                            Filter = "Archivo CSV (*.csv)|*.csv",
                            FileName = defaultFileName
                        };

                        if (saveDialog.ShowDialog() == true)
                        {
                            await File.WriteAllBytesAsync(saveDialog.FileName, fileBytes);
                            MessageBox.Show("El reporte se ha descargado exitosamente.", "Descarga Completa", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    else
                    {
                        string errorContent = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Error al descargar el reporte. Código: {response.StatusCode}. Mensaje: {errorContent}", "Error de Descarga", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error de conexión: {ex.Message}", "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

                // --- Validación de Fechas ---
                bool soloUnaFechaSeleccionada =
            (fechaInicialSeleccionada.HasValue && !fechaFinalSeleccionada.HasValue) ||
            (!fechaInicialSeleccionada.HasValue && fechaFinalSeleccionada.HasValue);

                if (soloUnaFechaSeleccionada)
                {
                    MessageBox.Show(
                        "Debe seleccionar tanto la Fecha Inicial como la Fecha Final para realizar la consulta.",
                        "Filtro Incompleto",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    dgVentas.ItemsSource = null;
                    return;
                }


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

                string estadoFiltroTexto = (cbEstadoVenta.SelectedItem as ComboBoxItem)?.Content?.ToString();

                if (string.IsNullOrEmpty(estadoFiltroTexto) || estadoFiltroTexto == "Todas")
                {
                    estadoFiltroTexto = string.Empty; // Envía cadena vacía para que Java lo ignore.
                }

                try
                {
                    var (pedidosDTO, codigo, mensaje) = await _servicioEstadisticas.ConsultarVentasAsync(
                        fechaInicial, fechaFinal, estadoFiltroTexto, _token
                    );

                    if (pedidosDTO == null || codigo != HttpStatusCode.OK && codigo != HttpStatusCode.NoContent)
                    {
                        // Si es 404 (NotFound)
                        if (codigo == HttpStatusCode.NotFound)
                        {
                            dgVentas.ItemsSource = null;
                            // Mostrar aviso de que no se encontraron resultados
                            MessageBox.Show("No se encontraron ventas con los filtros seleccionados. Intenta con otro rango de fechas o estado.", "Sin Resultados", MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }
                        dgVentas.ItemsSource = null;
                        MessageBox.Show($"Error al cargar ventas (HTTP {codigo}): {mensaje}", "Error de API", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (pedidosDTO.Count == 0)
                    {
                        dgVentas.ItemsSource = null;
                        MessageBox.Show("No se encontraron ventas con los filtros seleccionados.", "Sin Resultados", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    var ventasParaMostrar = pedidosDTO.Select(p => new Venta
                    {
                        TipoPedido = p.personalizado ? "Personalizado" : "Estandar",
                        Estado = _estadosApiMap.GetValueOrDefault(p.estado, "Desconocido"),

                        NombreRol = _rolMap.GetValueOrDefault(p.idRol, "Error al obtener el rol"),

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