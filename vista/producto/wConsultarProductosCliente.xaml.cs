using sweet_temptation_clienteEscritorio.dto;
using sweet_temptation_clienteEscritorio.resources;
using sweet_temptation_clienteEscritorio.servicios;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging; 

namespace sweet_temptation_clienteEscritorio.vista.producto
{
    public partial class wConsultarProductos : Page
    {
        private ProductoService _servicioProducto;
        private ProductoPedidoService _servicioProductoPedido;
        private PedidoService _servicioPedido;
        private ArchivoService _servicioArchivo;
        private readonly int _idPedidoEmpleado;

        private string _token;
        private int _idUsuario;
        private string _rolUsuario;

        // Variables para el modal
        private decimal _precioUnitarioActual;
        private int _stockMaximoActual;
        private int _idProductoActual;

        public ObservableCollection<ProductoVistaItem> ListaProductos { get; set; }
        private List<ProductoVistaItem> _listaCompletaCache;

        public wConsultarProductos(int idPedidoSeleccionado)
        {
            InitializeComponent();

            var http = new HttpClient();
            _servicioProducto = new ProductoService(http);
            _servicioProductoPedido = new ProductoPedidoService(http);
            _servicioPedido = new PedidoService(http);
            _servicioArchivo = new ArchivoService(http);
            _idPedidoEmpleado = idPedidoSeleccionado;

            if (App.Current.Properties.Contains("Token"))
                _token = (string?)App.Current.Properties["Token"];

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

            ListaProductos = new ObservableCollection<ProductoVistaItem>();
            ItemsControlProductos.ItemsSource = ListaProductos;

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
                        var model = new ProductoVistaItem
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

                        ListaProductos.Add(model);
                        _listaCompletaCache.Add(model);
                    }

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
            var copiaLista = ListaProductos.ToList();

            foreach (var item in copiaLista)
            {
                try
                {
                    var detalles = await _servicioArchivo.ObtenerDetallesArchivoAsync(item.IdProducto, _token);

                    if (detalles.detalles != null && !string.IsNullOrWhiteSpace(detalles.detalles.ruta))
                    {
                        
                        var resultado = await _servicioArchivo.ObtenerImagenAsync(detalles.detalles.ruta, _token);

                        if (resultado.imagen != null)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                var itemReal = ListaProductos.FirstOrDefault(p => p.IdProducto == item.IdProducto);
                                if (itemReal != null)
                                {
                                    itemReal.ImagenProducto = resultado.imagen;
                                }
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error imagen {item.IdProducto}: {ex.Message}");
                }
            }
        }

        private async Task CargarCategoriasAsync()
        {
            try
            {
                var respuesta = await _servicioProducto.ObtenerCategoriasAsync(_token);
                if (respuesta.codigo == System.Net.HttpStatusCode.OK && respuesta.categorias != null)
                {
                    var lista = new List<CategoriaDTO>
                    {
                        new CategoriaDTO { id = -1, nombre = "Seleccionar categoría…" },
                        new CategoriaDTO { id = 0, nombre = "Todas las categorías" }
                    };
                    lista.AddRange(respuesta.categorias);

                    CmbCategorias.ItemsSource = lista;
                    CmbCategorias.DisplayMemberPath = "nombre";
                    CmbCategorias.SelectedValuePath = "id";
                    CmbCategorias.SelectedIndex = 0;
                }
            }
            catch { }
        }

        private void BtnVerDetalle_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var producto = button?.Tag as ProductoVistaItem;

            if (producto != null)
            {
                _precioUnitarioActual = producto.Precio;
                _stockMaximoActual = producto.Unidades;
                _idProductoActual = producto.IdProducto;

                TxtDetalleNombre.Text = producto.Nombre;
                TxtDetalleDescripcion.Text = producto.Descripcion;
                ImgDetalle.ImageSource = producto.ImagenProducto;

                TxtCantidad.Text = "1";
                ActualizarPrecio(1);
                OverlayDetalle.Visibility = Visibility.Visible;
            }
        }

        private void ActualizarPrecio(int cantidad)
        {
            TxtDetallePrecioTotal.Text = (cantidad * _precioUnitarioActual).ToString("C");
        }

        private void BtnCerrarModal_Click(object sender, RoutedEventArgs e) => OverlayDetalle.Visibility = Visibility.Collapsed;
        private void BtnCerrarModal_Click(object sender, System.Windows.Input.MouseButtonEventArgs e) => OverlayDetalle.Visibility = Visibility.Collapsed;

        private void BtnMas_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(TxtCantidad.Text, out int cant) && cant < _stockMaximoActual)
            {
                cant++;
                TxtCantidad.Text = cant.ToString();
                ActualizarPrecio(cant);
            }
            else MessageBox.Show($"Solo hay {_stockMaximoActual} disponibles.");
        }

        private void BtnMenos_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(TxtCantidad.Text, out int cant) && cant > 1)
            {
                cant--;
                TxtCantidad.Text = cant.ToString();
                ActualizarPrecio(cant);
            }
        }

        private async void BtnAgregarAlPedido_Click(object sender, RoutedEventArgs e)
        {
            // Validaciones
            if (string.IsNullOrEmpty(_token))
            {
                MessageBox.Show("Sesión no válida, por favor inicie sesión.");
                return;
            }

            if (!int.TryParse(TxtCantidad.Text, out int cantidad) || cantidad <= 0)
            {
                MessageBox.Show("Cantidad inválida.");
                return;
            }

            var prod = _listaCompletaCache.FirstOrDefault(p => p.IdProducto == _idProductoActual);
            if (prod == null || !prod.Disponible)
            {
                MessageBox.Show("El producto ya no está disponible.");
                return;
            }

            try
            {
                BtnAgregarConfirmar.IsEnabled = false;

                if (_rolUsuario == "Empleado")
                {
                    if (_idPedidoEmpleado <= 0)
                    {
                        MessageBox.Show("Por favor selecciona un pedido");
                        return;
                    }

                    var respuesta = await _servicioProductoPedido.crearProductoAsync(prod.IdProducto, _idPedidoEmpleado, cantidad, _token);
                    if (respuesta.codigo == System.Net.HttpStatusCode.OK && respuesta.productoNuevo != null)
                    {
                        MessageBox.Show("Producto agregado");
                    }
                    else
                    {
                        MessageBox.Show("Error al guardar el producto al carrito: Error " + respuesta.mensaje);
                    }

                }
                else if (_rolUsuario == "Cliente")
                {
                    var pedidoRes = await _servicioPedido.ObtenerPedidoActualAsync(_idUsuario, _token);
                    if (pedidoRes.pedidoActual == null)
                    {
                       var creado = _servicioPedido.CrearPedidoClienteAsync(_idUsuario, _token);
                       var pedidoNuevo = await _servicioPedido.ObtenerPedidoActualAsync(_idUsuario, _token);
                       int idPedido = pedidoNuevo.pedidoActual.id;
                       var respuestaCliente = await _servicioProductoPedido.crearProductoAsync(prod.IdProducto, idPedido, cantidad, _token);
                       if (respuestaCliente.codigo == System.Net.HttpStatusCode.OK)
                       {
                          MessageBox.Show("Producto agregado");
                       }
                       else
                       {
                          MessageBox.Show("Error al guardar el producto al carrito");
                       }
                    }
                    else
                    {
                        int idPedido = pedidoRes.pedidoActual.id;

                        var respuestaCliente = await _servicioProductoPedido.crearProductoAsync(prod.IdProducto, idPedido, cantidad, _token);
                        if (respuestaCliente.codigo == System.Net.HttpStatusCode.OK)
                        {
                            MessageBox.Show("Producto agregado");
                        }
                        else
                        {
                            MessageBox.Show("Error al guardar el producto al carrito");
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Rol de usuario no reconocido. No se puede crear el pedido.", "Error de Rol", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error de conexión: " + ex.Message);
            }
            finally
            {
                BtnAgregarConfirmar.IsEnabled = true;
            }
        }

        private void TxtBuscar_TextChanged(object sender, TextChangedEventArgs e) => AplicarFiltros();
        private void CmbCategorias_SelectionChanged(object sender, SelectionChangedEventArgs e) => AplicarFiltros();

        private void AplicarFiltros()
        {
            if (_listaCompletaCache == null) return;
            string txt = TxtBuscar.Text.ToLower();
            bool busca = !string.IsNullOrWhiteSpace(txt) && txt != "buscar postre...";
            int idCat = (CmbCategorias.SelectedItem as CategoriaDTO)?.id ?? 0;

            var filtrados = _listaCompletaCache.Where(p =>
                (!busca || p.Nombre.ToLower().Contains(txt)) &&
                (idCat <= 0 || p.IdCategoria == idCat)
            ).ToList();

            ItemsControlProductos.ItemsSource = filtrados;
        }

        private void TxtBuscar_GotFocus(object sender, RoutedEventArgs e) { if (TxtBuscar.Text == "Buscar postre...") TxtBuscar.Text = ""; }
        private void TxtBuscar_LostFocus(object sender, RoutedEventArgs e) { if (string.IsNullOrWhiteSpace(TxtBuscar.Text)) TxtBuscar.Text = "Buscar postre..."; }

        private void BtnRegresarClick(object sender, RoutedEventArgs e)
        {
            if(NavigationService.CanGoBack)
                NavigationService.GoBack();
        }
    }

    public class ProductoVistaItem : INotifyPropertyChanged
    {
        public int IdProducto { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal Precio { get; set; }
        public bool Disponible { get; set; }
        public int IdCategoria { get; set; }
        public int Unidades { get; set; }
        public string CategoriaNombre { get; set; }

        private ImageSource _img;
        public ImageSource ImagenProducto
        {
            get => _img;
            set { _img = value; OnPropertyChanged(); }
        }

        public string TextoDisponibilidad => Disponible ? "Disponible" : "Agotado";
        public bool EstaDisponible => Disponible;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string n = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}