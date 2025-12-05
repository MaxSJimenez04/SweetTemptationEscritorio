using sweet_temptation_clienteEscritorio.dto;
using sweet_temptation_clienteEscritorio.servicios;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace sweet_temptation_clienteEscritorio.vista.producto
{
    public partial class wConsultarProductos : Page
    {
        // servicios
        private ProductoService _servicioProducto;
        private ProductoPedidoService _servicioProductoPedido; 
        private PedidoService _servicioPedido;
        private ArchivoService _servicioArchivo;

        private string _token;
        private int _idUsuario = 3;

        private decimal _precioUnitarioActual;
        private int _stockMaximoActual;
        private int _idProductoActual; 

        public ObservableCollection<ProductoVistaItem> ListaProductos { get; set; }
        private List<ProductoVistaItem> _listaCompletaCache; // Para buscar productos

        public wConsultarProductos()
        {
            InitializeComponent();

            _servicioProducto = new ProductoService(new HttpClient());
            _servicioProductoPedido = new ProductoPedidoService(new HttpClient());
            _servicioPedido = new PedidoService(new HttpClient());
            _servicioArchivo = new ArchivoService(new HttpClient());

            if (App.Current.Properties.Contains("Token"))
            {
                _token = (string?)App.Current.Properties["Token"];
            }

            ListaProductos = new ObservableCollection<ProductoVistaItem>();
            ItemsControlProductos.ItemsSource = ListaProductos;

            // Cargar datos al iniciar
            Loaded += async (s, e) =>
            {
                await CargarCategoriasAsync();
                await CargarProductosAsync();
            };
        }

        private async Task CargarProductosAsync()
        {
            if (string.IsNullOrEmpty(_token)) return;

            try
            {
                var respuesta = await _servicioProducto.ObtenerProductosAsync(_token);

                if (respuesta.codigo == System.Net.HttpStatusCode.OK && respuesta.productos != null)
                {
                    ListaProductos.Clear();
                    _listaCompletaCache = new List<ProductoVistaItem>();

                    foreach (var itemDTO in respuesta.productos)
                    {
                        var prodVista = new ProductoVistaItem
                        {
                            IdProducto = itemDTO.IdProducto,
                            Nombre = itemDTO.Nombre,
                            Descripcion = itemDTO.Descripcion,
                            Precio = itemDTO.Precio,
                            Disponible = itemDTO.Disponible,
                            Unidades = itemDTO.Unidades,
                            IdCategoria = itemDTO.Categoria,
                            CategoriaNombre = "Categoría " + itemDTO.Categoria,

                            ImagenProducto = null 
                        };

                        ListaProductos.Add(prodVista);
                        _listaCompletaCache.Add(prodVista);
                    }

                    // para que no bloquee la interfaz mientras descarga
                    _ = CargarImagenesAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar productos: {ex.Message}");
            }
        }

        private async Task CargarImagenesAsync()
        {
            var listaCopia = ListaProductos.ToList();

            foreach (var item in listaCopia)
            {
                try
                {
                    var respuestaDetalles = await _servicioArchivo.ObtenerDetallesArchivoAsync(item.IdProducto, _token);

                    if (respuestaDetalles.detalles != null && !string.IsNullOrEmpty(respuestaDetalles.detalles.ruta))
                    {
                        var respuestaImagen = await _servicioArchivo.ObtenerImagenAsync(respuestaDetalles.detalles.ruta, _token);

                        if (respuestaImagen.imagen != null)
                        {
                            // para actualizar la interfaz
                            // Dispatcher porque es una tarea asíncrona
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                item.ImagenProducto = respuestaImagen.imagen;
                            });
                        }
                    }
                }
                catch
                {
                    // Si falla una imagen (ej. 404), la ignoramos y el producto se queda sin foto.
                    // No mostramos MessageBox para no interrumpir al usuario.
                }
            }
        }

        private async Task CargarCategoriasAsync()
        {
            var respuesta = await _servicioProducto.ObtenerCategoriasAsync(_token);

            if (respuesta.codigo == System.Net.HttpStatusCode.OK && respuesta.categorias != null)
            {
                var listaCategorias = new List<CategoriaDTO>();

                listaCategorias.Add(new CategoriaDTO { id = 0, nombre = "Todas las Categorías" });

                listaCategorias.AddRange(respuesta.categorias);

                CmbCategorias.ItemsSource = listaCategorias;
                CmbCategorias.DisplayMemberPath = "nombre";
                CmbCategorias.SelectedValuePath = "id";

                CmbCategorias.SelectedIndex = 0;
            }
        }

        // Botones
        private void BtnVerDetalle_Click(object sender, RoutedEventArgs e)
        {
            var boton = sender as Button;
            var productoSeleccionado = boton.Tag as ProductoVistaItem;

            if (productoSeleccionado != null)
            {
                _precioUnitarioActual = productoSeleccionado.Precio;
                _stockMaximoActual = productoSeleccionado.Unidades;
                _idProductoActual = productoSeleccionado.IdProducto; 

                TxtDetalleNombre.Text = productoSeleccionado.Nombre;
                TxtDetalleDescripcion.Text = productoSeleccionado.Descripcion;

                if (productoSeleccionado.ImagenProducto != null)
                {
                    ImgDetalle.ImageSource = productoSeleccionado.ImagenProducto;
                }
                else
                {
                    ImgDetalle.ImageSource = null; 
                }

                TxtCantidad.Text = "1";
                ActualizarTextoPrecioTotal(1);

                ImgDetalle.ImageSource = productoSeleccionado.ImagenProducto;
                OverlayDetalle.Visibility = Visibility.Visible;
            }
        }

        private void ActualizarTextoPrecioTotal(int cantidad)
        {
            decimal total = cantidad * _precioUnitarioActual;
            TxtDetallePrecioTotal.Text = total.ToString("C");
        }

        private void BtnCerrarModal_Click(object sender, RoutedEventArgs e)
        {
            OverlayDetalle.Visibility = Visibility.Collapsed;
        }

        private void BtnCerrarModal_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            OverlayDetalle.Visibility = Visibility.Collapsed;
        }

        // Barra de búsqueda
        private void TxtBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_listaCompletaCache == null) return;

            string filtro = TxtBuscar.Text.ToLower();

            if (string.IsNullOrWhiteSpace(filtro) || filtro == "buscar postre...")
            {
                ItemsControlProductos.ItemsSource = _listaCompletaCache;
            }
            else
            {
                var filtrados = _listaCompletaCache
                    .Where(p => p.Nombre.ToLower().Contains(filtro))
                    .ToList();

                ItemsControlProductos.ItemsSource = filtrados;
            }
        }

        private void TxtBuscar_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TxtBuscar.Text == "Buscar postre...") TxtBuscar.Text = "";
        }

        private void TxtBuscar_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtBuscar.Text)) TxtBuscar.Text = "Buscar postre...";
        }

        // Filtro Categoría
        private void CmbCategorias_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_listaCompletaCache == null || CmbCategorias.SelectedItem == null) return;

            var categoriaSeleccionada = (CategoriaDTO)CmbCategorias.SelectedItem;

            if (categoriaSeleccionada.id == 0)
            {
                ItemsControlProductos.ItemsSource = _listaCompletaCache;
            }
            else
            {
                var filtrados = _listaCompletaCache
                    .Where(p => p.IdCategoria == categoriaSeleccionada.id)
                    .ToList();

                ItemsControlProductos.ItemsSource = filtrados;
            }
            TxtBuscar.Text = "Buscar postre...";
        }

        private void BtnMas_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(TxtCantidad.Text, out int cantidadActual))
            {
                if (cantidadActual < _stockMaximoActual)
                {
                    cantidadActual++;
                    TxtCantidad.Text = cantidadActual.ToString();
                    ActualizarTextoPrecioTotal(cantidadActual);
                }
                else
                {
                    MessageBox.Show($"Solo hay {_stockMaximoActual} unidades disponibles de este producto.");
                }
            }
        }

        private void BtnMenos_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(TxtCantidad.Text, out int cantidadActual))
            {
                if (cantidadActual > 1)
                {
                    cantidadActual--;
                    TxtCantidad.Text = cantidadActual.ToString();
                    ActualizarTextoPrecioTotal(cantidadActual);
                }
            }
        }

        private async void BtnAgregarAlPedido_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_token))
            {
                MessageBox.Show("Sesión no válida.");
                return;
            }

            try
            {
                BtnAgregarConfirmar.IsEnabled = false;

                int cantidad = int.Parse(TxtCantidad.Text);

                var respuestaPedido = await _servicioPedido.ObtenerPedidoActualAsync(_idUsuario, _token);

                if (respuestaPedido.pedidoActual == null)
                {
                    await _servicioPedido.CrearPedidoClienteAsync(_idUsuario, _token);
                    
                    respuestaPedido = await _servicioPedido.ObtenerPedidoActualAsync(_idUsuario, _token);
                }

                if (respuestaPedido.pedidoActual != null)
                {
                    int idPedidoActual = respuestaPedido.pedidoActual.id;

                    var resultado = await _servicioProductoPedido.crearProductoAsync(
                        idProducto: _idProductoActual,
                        idPedido: idPedidoActual,
                        cantidad: cantidad,
                        token: _token
                    );

                    if (resultado.codigo == System.Net.HttpStatusCode.Created ||
                        resultado.codigo == System.Net.HttpStatusCode.OK)
                    {
                        MessageBox.Show("¡Producto agregado al carrito exitosamente!");
                        OverlayDetalle.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        MessageBox.Show($"No se pudo agregar: {resultado.mensaje}");
                    }
                }
                else
                {
                    MessageBox.Show("Error: No se pudo generar un número de pedido.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ocurrió un error: " + ex.Message);
            }
            finally
            {
                BtnAgregarConfirmar.IsEnabled = true;
            }
        }
    }

    
    public class ProductoVistaItem : INotifyPropertyChanged
    {
        public int IdProducto { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal Precio { get; set; }
        public bool Disponible { get; set; }
        public string CategoriaNombre { get; set; }
        public int IdCategoria { get; set; }
        public int Unidades { get; set; }

        private ImageSource _imagenProducto;
        public ImageSource ImagenProducto
        {
            get { return _imagenProducto; }
            set
            {
                _imagenProducto = value;
                OnPropertyChanged(); // actualiza la pantalla
            }
        }

        public bool EstaDisponible => Disponible;
        public string TextoDisponibilidad => Disponible ? "Disponible" : "Agotado";

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}