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

        private decimal _precioUnitarioActual;
        private int _stockMaximoActual;

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
            Loaded += async (s, e) =>
            {
                await CargarCategoriasAsync();
                await CargarProductosAsync();
            };
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
                            Unidades = itemDTO.Unidades,
                            IdCategoria = itemDTO.categoria,

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

        private async Task CargarCategoriasAsync()
        {
            // Llamamos al método que agregamos al servicio en el paso anterior
            var respuesta = await _servicioProducto.ObtenerCategoriasAsync(_token);

            if (respuesta.codigo == System.Net.HttpStatusCode.OK && respuesta.categorias != null)
            {
                var listaCategorias = new List<CategoriaDTO>();

                // 1. Agregamos la opción "Todas" (ID 0)
                // OJO: Usa minusculas (id, nombre) si así está en tu CategoriaDTO
                listaCategorias.Add(new CategoriaDTO { id = 0, nombre = "Todas las Categorías" });

                // 2. Agregamos las de la BD
                listaCategorias.AddRange(respuesta.categorias);

                // 3. Configuramos el ComboBox
                CmbCategorias.ItemsSource = listaCategorias;
                CmbCategorias.DisplayMemberPath = "nombre"; // Lo que se ve
                CmbCategorias.SelectedValuePath = "id";     // El valor oculto

                // 4. Seleccionamos la primera
                CmbCategorias.SelectedIndex = 0;
            }
        }


        // --BOTONES--
        private void BtnVerDetalle_Click(object sender, RoutedEventArgs e)
        {
            var boton = sender as Button;
            var productoSeleccionado = boton.Tag as ProductoVistaItem;

            if (productoSeleccionado != null)
            {
                // 1. Guardamos los datos importantes en las variables temporales
                _precioUnitarioActual = productoSeleccionado.Precio;
                _stockMaximoActual = productoSeleccionado.Unidades;

                // 2. Llenamos la UI
                TxtDetalleNombre.Text = productoSeleccionado.Nombre;
                TxtDetalleDescripcion.Text = productoSeleccionado.Descripcion;

                // Inicializamos siempre en 1 al abrir
                TxtCantidad.Text = "1";

                // Calculamos el total inicial (1 * precio)
                ActualizarTextoPrecioTotal(1);

                ImgDetalle.ImageSource = productoSeleccionado.ImagenProducto;
                OverlayDetalle.Visibility = Visibility.Visible;
            }
        }

        // Método auxiliar para actualizar el texto del precio total
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
        private void CmbCategorias_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Evitamos errores si aún no cargan los datos
            if (_listaCompletaCache == null || CmbCategorias.SelectedItem == null) return;

            // Obtenemos la categoría seleccionada
            var categoriaSeleccionada = (CategoriaDTO)CmbCategorias.SelectedItem;

            if (categoriaSeleccionada.id == 0)
            {
                // Si es "Todas", mostramos la lista completa
                ItemsControlProductos.ItemsSource = _listaCompletaCache;
            }
            else
            {
                // Filtramos comparando el ID de la categoría
                var filtrados = _listaCompletaCache
                    .Where(p => p.IdCategoria == categoriaSeleccionada.id)
                    .ToList();

                ItemsControlProductos.ItemsSource = filtrados;
            }

            // Reiniciamos el buscador de texto para no confundir
            TxtBuscar.Text = "Buscar postre...";
        }

        // Para restar al producto
        private void BtnMas_Click(object sender, RoutedEventArgs e)
        {
            int cantidadActual = int.Parse(TxtCantidad.Text);

            // Validar stock
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

        private void BtnMenos_Click(object sender, RoutedEventArgs e)
        {
            int cantidadActual = int.Parse(TxtCantidad.Text);

            // Validar - no puede haber menos de 1
            if (cantidadActual > 1)
            {
                cantidadActual--;
                TxtCantidad.Text = cantidadActual.ToString();
                ActualizarTextoPrecioTotal(cantidadActual);
            }
        }

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

        public int IdCategoria { get; set; }

        // para la tarjeta de modal
        public int Unidades { get; set; }

        public ImageSource ImagenProducto { get; set; }
        public bool EstaDisponible => Disponible;
        public string TextoDisponibilidad => Disponible ? "Disponible" : "Agotado";
    }
}