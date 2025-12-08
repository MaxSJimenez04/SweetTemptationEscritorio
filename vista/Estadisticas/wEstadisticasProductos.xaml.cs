using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using sweet_temptation_clienteEscritorio.dto;
using sweet_temptation_clienteEscritorio.servicios;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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


namespace sweet_temptation_clienteEscritorio.vista.Estadisticas
{
    public partial class wEstadisticasProductos : Page
    {
        private EstadisticasService _estadisticasService;
        private ProductoService _productoService;
        string _token;
        public List<Producto> _productos = new List<Producto>();
        public List<EstadisticaVentaProductoDTO> _estadisticasProducto = new List<EstadisticaVentaProductoDTO>();
       
        public wEstadisticasProductos()
        {
            _estadisticasService = new EstadisticasService(new HttpClient());
            _productoService = new ProductoService(new HttpClient());
            InitializeComponent();
            setRangosFecha();
            _token = (string?)App.Current.Properties["Token"];
            Loaded += async (s, e) =>
            {
                await ObtenerProductosAsync();

            };
        }

        private void setRangosFecha()
        {
            cbRangoFecha.Items.Clear();
            cbRangoFecha.Items.Add("Mes pasado");
            cbRangoFecha.Items.Add("Semana pasada");
            cbRangoFecha.Items.Add("Quincena pasada");
        }


        private async Task ObtenerProductosAsync()
        {
            var respuesta = await _productoService.ObtenerProductosAsync(_token);
            if (respuesta.codigo == System.Net.HttpStatusCode.OK && respuesta.productos != null)
            {
               
                foreach (var producto in respuesta.productos)
                {
                    _productos.Add(new Producto()
                    {
                        IdProducto = producto.IdProducto,
                        Nombre = producto.Nombre,
                    });
                    
                }
                cbNombreProducto.Items.Clear();
                cbNombreProducto.ItemsSource = _productos;
            }
            else
            {
                MessageBox.Show(respuesta.mensaje);
                cbNombreProducto.IsEnabled = false;
            }
        }

        private async Task ObtenerEstadisticasProductosAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            var respuesta = await _estadisticasService.ObtenerEstadisticasProductosAsync(fechaInicio, fechaFin, _token);
            if (respuesta.codigo == System.Net.HttpStatusCode.OK && respuesta.productos != null)
            {
                if (!respuesta.productos.Any())
                {
                    MessageBox.Show("No hay estadisticas para mostrar en el rango seleccionado");
                    tbMejoresProductos.ItemsSource = null;
                    tbPeoresProductos.ItemsSource = null;
                    return;
                }
                else
                {
                    var mejoresProductos = new List<EstadisticaProductoDTO>();
                    var peoresProductos = new List<EstadisticaProductoDTO>();
                    
                    foreach (var item in respuesta.productos)
                    {
                        if (item.categoria == "MAS VENDIDOS")
                            mejoresProductos.Add(item);
                        else if (item.categoria == "MENOS VENDIDOS")
                            peoresProductos.Add(item);
                    }

                    tbMejoresProductos.ItemsSource = mejoresProductos;
                    tbPeoresProductos.ItemsSource = peoresProductos;

                    tbMejoresProductos.Items.Refresh();
                    tbPeoresProductos.Items.Refresh();
                }

            }
            else
            {
                MessageBox.Show("Error: " + respuesta.mensaje);
            }

        }

        public struct RangoFecha
        {
            public DateTime FechaInicio { get; set; }
            public DateTime FechaFin { get; set; }
        }

        public static RangoFecha CalcularRangoFecha(string tipoRango)
        {
            DateTime hoy = DateTime.Today;
            DateTime inicio;
            DateTime fin;

            switch (tipoRango)
            {
                case "Mes pasado":
                    var mesPasado = hoy.AddMonths(-1);
                    inicio = new DateTime(mesPasado.Year, mesPasado.Month, 1);
                    fin = inicio.AddMonths(1).AddDays(-1); // último día del mes pasado
                    break;

                case "Semana pasada":
                    // Asumimos semana: lunes–domingo
                    int diff = (7 + (int)hoy.DayOfWeek - (int)DayOfWeek.Monday) % 7;
                    DateTime inicioSemanaActual = hoy.AddDays(-diff);
                    inicio = inicioSemanaActual.AddDays(-7); // lunes semana pasada
                    fin = inicioSemanaActual.AddDays(-1);    // domingo semana pasada
                    break;

                case "Quincena pasada":
                    if (hoy.Day <= 15)
                    {
                        // Quincena pasada: 16 a fin del mes anterior
                        var mesAnterior = hoy.AddMonths(-1);
                        inicio = new DateTime(mesAnterior.Year, mesAnterior.Month, 16);
                        int diasMesAnterior = DateTime.DaysInMonth(mesAnterior.Year, mesAnterior.Month);
                        fin = new DateTime(mesAnterior.Year, mesAnterior.Month, diasMesAnterior);
                    }
                    else
                    {
                        // Quincena pasada: 1 a 15 del mes actual
                        inicio = new DateTime(hoy.Year, hoy.Month, 1);
                        fin = new DateTime(hoy.Year, hoy.Month, 15);
                    }
                    break;

                default:
                    throw new ArgumentException("Tipo de rango no soportado", nameof(tipoRango));
            }

            return new RangoFecha
            {
                FechaInicio = inicio,
                FechaFin = fin
            };
        }

        private async Task ObtenerEstadisticasProductoAsync(DateTime fechaInicio, DateTime fechaFin, int idProducto)
        {
            var respuesta = await _estadisticasService.ObtenerEstadisticasProductoAsync(fechaInicio, fechaFin, idProducto, _token);

            if (respuesta.mensaje != null)
            {
                if (respuesta.estadisticas != null)
                {
                    _estadisticasProducto.Clear();
                    foreach (var estadistica in respuesta.estadisticas)
                    {
                        _estadisticasProducto.Add(estadistica);
                    }
                }
                else
                {
                    MessageBox.Show("No se encontraron ventas para el producto seleccionado");
                }
            }
            else
            {
                MessageBox.Show("Error" + respuesta.mensaje);
            }
        }

        private async void cbRangoFecha_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            tbMejoresProductos.ItemsSource = null;
            tbPeoresProductos.ItemsSource = null;
            tbMejoresProductos.Items.Clear();
            tbPeoresProductos.Items.Clear();

            txtPeriodo.Text = "Periodo: ";

            string opcion = cbRangoFecha.SelectedItem.ToString();
            RangoFecha fechas = CalcularRangoFecha(opcion);

            string fechaInicioStr = fechas.FechaInicio.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            string fechaFinStr = fechas.FechaFin.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            txtPeriodo.Text += $" {fechaInicioStr} - {fechaFinStr}";
            switch (opcion)
            {
                case "Mes pasado":
                    await ObtenerEstadisticasProductosAsync(fechas.FechaInicio, fechas.FechaFin);
                    break;
                case "Semana pasada":
                    await ObtenerEstadisticasProductosAsync(fechas.FechaInicio, fechas.FechaFin);
                    break;
                case "Quincena pasada":
                    await ObtenerEstadisticasProductosAsync(fechas.FechaInicio, fechas.FechaFin);
                    break;
            }
        }

        /*private void crearGrafico(IEnumerable<EstadisticaVentaProductoDTO> datos)
        {
            var serieOrdenada = datos.OrderBy(d => d.fecha).ToList();
            var lineSeries = new LineSeries<DateTimePoint>
            {
                Name = "Cantidad",
                Values = (IReadOnlyCollection<DateTimePoint>)serieOrdenada.Select(d =>
                    new DateTimePoint(d.fecha, d.ventasPorDia)),

                GeometrySize = 8,
                Stroke = new SolidColorPaint(SKColors.LightPink) { StrokeThickness = 2 },
                Fill = null
            };

            Series = new[] { lineSeries };

            EjesX = new[]
            {
                    new Axis
                    {
                        Labeler = value => DateTime.FromOADate(value).ToString("dd/MM/yy"),
                        LabelsRotation = 45,
                        UnitWidth = TimeSpan.FromDays(1).Ticks,
                        Name = "Fecha"
                    }
                };

            EjesY = new[]
            {
                    new Axis
                    {
                        Name = "Cantidad",
                        MinLimit = 0,
                    }
                };
        }*/

        private async void cbNombreProducto_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int idSeleccionado;
            try
            {
                idSeleccionado = (int)cbNombreProducto.SelectedValue;
            }
            catch
            {
                if (!int.TryParse(cbNombreProducto.SelectedValue.ToString(), out idSeleccionado))
                    return;
            }

            string opcion = cbRangoFecha.SelectedItem?.ToString() ?? string.Empty;
            RangoFecha fechas = CalcularRangoFecha(opcion);

            await ObtenerEstadisticasProductoAsync(fechas.FechaInicio, fechas.FechaFin, idSeleccionado);

            if (_estadisticasProducto != null && _estadisticasProducto.Any())
            {
                IEnumerable<EstadisticaVentaProductoDTO> estadisticasProducto = _estadisticasProducto;
                //crearGrafico(estadisticasProducto);
            }

        }
    }
}
