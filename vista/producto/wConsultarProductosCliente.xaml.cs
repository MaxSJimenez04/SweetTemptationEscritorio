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
using System.Net.Http; // Necesario para la comunicación con la API
using System.Text.Json; // Necesario para la deserialización
using sweet_temptation_clienteEscritorio.dto; // Asegúrate de que este DTO esté disponible

namespace sweet_temptation_clienteEscritorio.vista.producto
{
    /// <summary>
    /// Lógica de interacción para wConsultarProductos.xaml
    /// </summary>
    public partial class wConsultarProductos : Page
    {
        // ** CONSTANTE DE LA URL **
        private const string API_PRODUCTOS_TODOS_URL = "http://localhost:8080/producto/todos";
        private const string API_CATEGORIAS_URL = "http://localhost:8080/categoria/todos";

        private HttpClient _httpClient;
        private List<ProductoDTO> _listaProductosOriginal; // Lista completa para filtros
        private ProductoDTO _productoSeleccionado;
        private int _cantidadPedido = 1; 

        public wConsultarProductos()
        {
            InitializeComponent();
            _httpClient = new HttpClient();

            // Inicia la carga de datos al inicializar la vista
            _ = InicializarDatosAsync();
        }

        /// <summary>
        /// Inicializa la carga de productos y categorías
        /// </summary>
        private async Task InicializarDatosAsync()
        {
            await CargarCategoriasAsync();
            await CargarProductosAsync();
        }

        /// <summary>
        /// Método para cargar las categorías de la API al ComboBox.
        /// (Se asume la clase Categoria y CategoriaDTO existen)
        /// </summary>
        private async Task CargarCategoriasAsync()
        {
            try
            {
                HttpResponseMessage respuesta = await _httpClient.GetAsync(API_CATEGORIAS_URL);

                if (respuesta.IsSuccessStatusCode)
                {
                    string jsonCategorias = await respuesta.Content.ReadAsStringAsync();
                    var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };


                    List<CategoriaDTO> categorias = JsonSerializer.Deserialize<List<CategoriaDTO>>(jsonCategorias, opciones);

                    CmbCategorias.ItemsSource = new List<CategoriaDTO> { new CategoriaDTO { id = 0, nombre = "Todas las Categorías" } }
                                                .Concat(categorias).ToList();
                    CmbCategorias.DisplayMemberPath = "Nombre";
                    CmbCategorias.SelectedValuePath = "Id";
                    CmbCategorias.SelectedIndex = 0; // Seleccionar "Todas las Categorías"
                }
            }
            catch (HttpRequestException ex)
            {
                System.Windows.MessageBox.Show($"🚨 Error de conexión al cargar categorías: {ex.Message}", "Error de Red");
            }
        }


        /// <summary>
        /// Método para cargar la lista de productos desde la API y mostrarlos en la interfaz.
        /// </summary>
        private async Task CargarProductosAsync()
        {
            try
            {
                HttpResponseMessage respuesta = await _httpClient.GetAsync(API_PRODUCTOS_TODOS_URL);

                if (respuesta.IsSuccessStatusCode)
                {
                    string jsonProductos = await respuesta.Content.ReadAsStringAsync();

                    var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    _listaProductosOriginal = JsonSerializer.Deserialize<List<ProductoDTO>>(jsonProductos, opciones);

                    ItemsControlProductos.ItemsSource = _listaProductosOriginal;

                    System.Windows.MessageBox.Show($"Productos cargados: {_listaProductosOriginal.Count}", "Éxito de Carga");

                }
                else
                {
                    System.Windows.MessageBox.Show($"Error al obtener productos: {respuesta.StatusCode}", "Error de API", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (HttpRequestException ex)
            {
                System.Windows.MessageBox.Show($"🚨 Error de conexión: No se pudo conectar a la API en {API_PRODUCTOS_TODOS_URL}.\nDetalle: {ex.Message}", "Error de Red", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"🔴 Ocurrió un error inesperado al cargar productos: {ex.Message}", "Error General", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Maneja el evento de clic para ABRIR el modal de detalle del producto seleccionado.
        /// </summary>
        private void BtnVerDetalle_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null && btn.Tag is ProductoDTO producto)
            {
                _productoSeleccionado = producto;
                _cantidadPedido = 1; // Reiniciar cantidad

                TxtDetalleNombre.Text = producto.Nombre;
                TxtDetalleDescripcion.Text = producto.Descripcion;
                TxtCantidad.Text = _cantidadPedido.ToString();

                ActualizarDetalleModal();

                OverlayDetalle.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Maneja el evento de clic para CERRAR el modal de detalle.
        /// </summary>
        private void BtnCerrarModal_Click(object sender, RoutedEventArgs e)
        {
            OverlayDetalle.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Maneja el evento de clic para disminuir la cantidad de un producto en el pedido.
        /// </summary>
        private void BtnMenos_Click(object sender, RoutedEventArgs e)
        {
            if (_cantidadPedido > 1)
            {
                _cantidadPedido--;
                ActualizarDetalleModal();
            }
        }

        /// <summary>
        /// Maneja el evento de clic para aumentar la cantidad de un producto.
        /// </summary>
        private void BtnMas_Click(object sender, RoutedEventArgs e)
        {
            _cantidadPedido++;
            ActualizarDetalleModal();
        }

        /// <summary>
        /// Maneja el evento de clic para agregar el producto con la cantidad seleccionada al pedido.
        /// </summary>
        private void BtnAgregarAlPedido_Click(object sender, RoutedEventArgs e)
        {
            if (_productoSeleccionado != null && _cantidadPedido > 0)
            {
                System.Windows.MessageBox.Show($"Agregado al pedido: {_productoSeleccionado.Nombre} x {_cantidadPedido}", "Pedido");

                OverlayDetalle.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Maneja el evento GotFocus del TextBox de búsqueda (limpia el texto placeholder).
        /// </summary>
        private void TxtBuscar_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TxtBuscar.Text == "Buscar postre...")
            {
                TxtBuscar.Text = string.Empty;
            }
        }

        /// <summary>
        /// Maneja el evento LostFocus del TextBox de búsqueda (restaura el texto placeholder si está vacío).
        /// </summary>
        private void TxtBuscar_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtBuscar.Text))
            {
                TxtBuscar.Text = "Buscar postre...";
            }
        }

        /// <summary>
        /// Maneja los cambios de texto para filtrar la lista de productos.
        /// </summary>
        private void TxtBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            AplicarFiltros();
        }

        /// <summary>
        /// Maneja el cambio de selección en el ComboBox de categorías para filtrar la lista.
        /// </summary>
        private void CmbCategorias_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AplicarFiltros();
        }


        /// <summary>
        /// Aplica los filtros de búsqueda y categoría a la lista de productos.
        /// </summary>
        private void AplicarFiltros()
        {
            if (_listaProductosOriginal == null) return;

            IEnumerable<ProductoDTO> productosFiltrados = _listaProductosOriginal;

            // 1. Filtro por Búsqueda (Texto)
            string textoBusqueda = TxtBuscar.Text.ToLowerInvariant();
            if (!string.IsNullOrWhiteSpace(textoBusqueda) && textoBusqueda != "buscar postre...")
            {
                productosFiltrados = productosFiltrados.Where(p =>
                    p.Nombre.ToLowerInvariant().Contains(textoBusqueda) ||
                    p.Descripcion.ToLowerInvariant().Contains(textoBusqueda));
            }

            // 2. Filtro por Categoría
            if (CmbCategorias.SelectedValue is int idCategoria && idCategoria > 0)
            {
                productosFiltrados = productosFiltrados.Where(p => p.categoria == idCategoria);
            }

            // Actualizar la interfaz
            ItemsControlProductos.ItemsSource = productosFiltrados.ToList();
        }

        /// <summary>
        /// Actualiza la cantidad y el precio total mostrado en el modal de detalle.
        /// </summary>
        private void ActualizarDetalleModal()
        {
            if (_productoSeleccionado != null)
            {
                // Actualizar la cantidad
                TxtCantidad.Text = _cantidadPedido.ToString();

                decimal precioUnitario = _productoSeleccionado.Precio;
                decimal precioTotal = precioUnitario * _cantidadPedido;

                TxtDetallePrecioTotal.Text = string.Format("{0:C}", precioTotal);

                // Habilitar/Deshabilitar el botón de agregar si hay suficiente stock (opcional)
                // BtnAgregarConfirmar.IsEnabled = (_cantidadPedido <= _productoSeleccionado.Unidades);
            }
        }

        private void BtnModificar_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("Funcionalidad pendiente: Modificar Producto.", "Información");
        }

        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("Funcionalidad pendiente: Eliminar Producto.", "Información");
        }
    }
}
