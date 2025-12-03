using sweet_temptation_clienteEscritorio.dto; 
using sweet_temptation_clienteEscritorio.servicios;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace sweet_temptation_clienteEscritorio.vista.producto
{
    public partial class wConsultarProductos : Page
    {
        private ProductoService _servicioProducto;
        private string _token;
        public ObservableCollection<ProductoVistaItem> ListaProductos { get; set; }

        // para buscar productos
        private List<ProductoVistaItem> _listaCompletaCache;

        public wConsultarProductos()
        {
            InitializeComponent();

            _servicioProducto = new ProductoService(new HttpClient());

            if (App.Current.Properties.Contains("Token"))
            {
                _token = (string?)App.Current.Properties["Token"];
            }

            ListaProductos = new ObservableCollection<ProductoVistaItem>();
            ItemsControlProductos.ItemsSource = ListaProductos;

            // cargar productos
            Loaded += async (s, e) => await CargarProductosAsync();
        }

        private async Task CargarProductosAsync()
        {
            if (string.IsNullOrEmpty(_token))
            {
                MessageBox.Show("No se ha detectado una sesión activa. Por favor inicie sesión.");
                return;
            }

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

                            // TODO - cambiar la categoria, hay que traerlo de la base de datos - ya esta en la api
                            CategoriaNombre = "Categoría " + itemDTO.categoria,

                            // TODO - la imagen esta en null, pero hay que cambiarlo
                            ImagenProducto = null
                        };

                        ListaProductos.Add(prodVista);
                        _listaCompletaCache.Add(prodVista);
                    }
                }
                else
                {
                    MessageBox.Show($"No se pudieron cargar los productos: {respuesta.mensaje}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error inesperado: {ex.Message}");
            }
        }

        private void BtnVerDetalle_Click(object sender, RoutedEventArgs e)
        {
            var boton = sender as Button;
            var productoSeleccionado = boton.Tag as ProductoVistaItem;

            if (productoSeleccionado != null)
            {
                TxtDetalleNombre.Text = productoSeleccionado.Nombre;
                TxtDetalleDescripcion.Text = productoSeleccionado.Descripcion;
                TxtDetallePrecioTotal.Text = productoSeleccionado.Precio.ToString("C"); // Formato moneda
                TxtCantidad.Text = "1";

                // TODO - Aqui agregar la imagen
                ImgDetalle.ImageSource = productoSeleccionado.ImagenProducto;

                // Aqui se muestra la tarjeta del producto
                OverlayDetalle.Visibility = Visibility.Visible;
            }
        }

        private void BtnCerrarModal_Click(object sender, RoutedEventArgs e)
        {
            OverlayDetalle.Visibility = Visibility.Collapsed;
        }

        private void BtnCerrarModal_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            OverlayDetalle.Visibility = Visibility.Collapsed;
        }

        // Barra de busqueda
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
            if (TxtBuscar.Text == "Buscar postre...")
                TxtBuscar.Text = "";
        }

        private void TxtBuscar_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtBuscar.Text))
                TxtBuscar.Text = "Buscar postre...";
        }

        // TODO - Agregar funcionalidad
        private void CmbCategorias_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

        // Para restar al producto
        private void BtnMenos_Click(object sender, RoutedEventArgs e) {  }
        
        // Para agregar unidades al producto
        private void BtnMas_Click(object sender, RoutedEventArgs e) { }

        // Agregar el producto al pedido
        private void BtnAgregarAlPedido_Click(object sender, RoutedEventArgs e) {}
    }

    // Ayuda para el modelo dto
    // Para mostrar disponible y agotado
    public class ProductoVistaItem
    {
        public int IdProducto { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal Precio { get; set; }
        public bool Disponible { get; set; }
        public string CategoriaNombre { get; set; }
        public ImageSource ImagenProducto { get; set; }
        public bool EstaDisponible => Disponible;
        public string TextoDisponibilidad => Disponible ? "Disponible" : "Agotado";
    }
}