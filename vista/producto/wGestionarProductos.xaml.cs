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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace sweet_temptation_clienteEscritorio.vista.producto
{
    public partial class wAdministrarProductos : Page
    {
        // Servicios
        private ProductoService _servicioProducto;
        private ArchivoService _servicioArchivo;

        private string _token;

        // Lista principal
        public ObservableCollection<ProductoVistaAdminItem> ListaProductos { get; set; }

        // Para filtros
        private List<ProductoVistaAdminItem> _listaCompletaCache;

        public wAdministrarProductos()
        {
            InitializeComponent();

            _servicioProducto = new ProductoService(new HttpClient());
            _servicioArchivo = new ArchivoService(new HttpClient());

            if (App.Current.Properties.Contains("Token"))
                _token = (string?)App.Current.Properties["Token"];

            ListaProductos = new ObservableCollection<ProductoVistaAdminItem>();
            DataGridProductos.ItemsSource = ListaProductos;

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
                MessageBox.Show("No se ha detectado una sesión activa.");
                return;
            }

            try
            {
                var respuesta = await _servicioProducto.ObtenerProductosAsync(_token);

                if (respuesta.codigo == System.Net.HttpStatusCode.OK && respuesta.productos != null)
                {
                    ListaProductos.Clear();
                    _listaCompletaCache = new List<ProductoVistaAdminItem>();

                    foreach (var itemDTO in respuesta.productos)
                    {
                        var prod = new ProductoVistaAdminItem
                        {
                            IdProducto = itemDTO.IdProducto,
                            Nombre = itemDTO.Nombre,
                            Descripcion = itemDTO.Descripcion,
                            Precio = itemDTO.Precio,
                            Disponible = itemDTO.Disponible,
                            Unidades = itemDTO.Unidades,
                            FechaRegistro = itemDTO.FechaRegistro,
                            IdCategoria = itemDTO.Categoria
                        };

                        ListaProductos.Add(prod);
                        _listaCompletaCache.Add(prod);
                    }
                }
                else
                {
                    MessageBox.Show($"Error: {respuesta.mensaje}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar productos: " + ex.Message);
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
                        new CategoriaDTO { id = 0, nombre = "Categorías" }
                    };

                    lista.AddRange(respuesta.categorias);

                    CmbCategoriasFiltro.ItemsSource = lista;
                    CmbCategoriasFiltro.DisplayMemberPath = "nombre";
                    CmbCategoriasFiltro.SelectedValuePath = "id";

                    CmbCategoriasFiltro.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar categorías: " + ex.Message);
            }
        }

        private void FiltrarProductos()
        {
            if (_listaCompletaCache == null) return;

            string filtroTexto = TxtBuscar.Text.ToLower();
            var categoriaSeleccionada = CmbCategoriasFiltro.SelectedItem as CategoriaDTO;

            var listaFiltrada = _listaCompletaCache.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(filtroTexto) && filtroTexto != "buscar producto...")
            {
                listaFiltrada = listaFiltrada.Where(p =>
                    p.Nombre.ToLower().Contains(filtroTexto) ||
                    p.Descripcion.ToLower().Contains(filtroTexto)
                );
            }

            if (categoriaSeleccionada != null && categoriaSeleccionada.id != 0)
            {
                listaFiltrada = listaFiltrada.Where(p =>
                    p.IdCategoria == categoriaSeleccionada.id
                );
            }

            DataGridProductos.ItemsSource =
                new ObservableCollection<ProductoVistaAdminItem>(listaFiltrada);
        }

        private void TxtBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            FiltrarProductos();
        }

        private void TxtBuscar_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TxtBuscar.Text == "Buscar producto...")
                TxtBuscar.Text = "";
        }

        private void TxtBuscar_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtBuscar.Text))
                TxtBuscar.Text = "Buscar producto...";
        }

        private void CmbCategoriasFiltro_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FiltrarProductos();
        }

        private void BtnAgregarProducto_Click(object sender, RoutedEventArgs e)
        {
            var ventana = Window.GetWindow(this) as wndMenuEmpleado; 

            if (ventana != null)
            {
                ventana.fmPrincipal.Navigate(new sweet_temptation_clienteEscritorio.vista.producto.wRegistrarProducto());
            }
        }

        private void BtnModificar_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var producto = btn.Tag as ProductoVistaAdminItem;

            MessageBox.Show($"Modificar producto ID: {producto.IdProducto}");
        }

        private async void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var producto = btn.Tag as ProductoVistaAdminItem;

            if (producto == null) return;

            var confirmar = MessageBox.Show(
                $"¿Desea eliminar el producto '{producto.Nombre}'?",
                "Confirmar eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmar == MessageBoxResult.Yes)
            {
                var respuesta = await _servicioProducto.EliminarProductoAsync(producto.IdProducto, _token);

                if (respuesta.codigo == System.Net.HttpStatusCode.OK)
                {
                    ListaProductos.Remove(producto);
                    _listaCompletaCache.Remove(producto);
                    MessageBox.Show("Producto eliminado correctamente.");
                }
                else
                {
                    MessageBox.Show("Error al eliminar: " + respuesta.mensaje);
                }
            }
        }
    }
    public class ProductoVistaAdminItem : INotifyPropertyChanged
    {
        public int IdProducto { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal Precio { get; set; }
        public bool Disponible { get; set; }
        public int Unidades { get; set; }
        public int IdCategoria { get; set; }
        public DateTime? FechaRegistro { get; set; }

        public string TextoDisponibilidad => Disponible ? "Sí" : "No";

        private ImageSource _imagenProducto;
        public ImageSource ImagenProducto
        {
            get => _imagenProducto;
            set { _imagenProducto = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string property = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}

