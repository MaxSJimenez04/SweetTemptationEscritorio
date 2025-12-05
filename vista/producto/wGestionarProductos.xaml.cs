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

        // Lista principal enlazada al DataGrid
        public ObservableCollection<ProductoVistaAdminItem> ListaProductos { get; set; }

        // Cache para filtros
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

                    foreach (var dto in respuesta.productos)
                    {
                        var prod = new ProductoVistaAdminItem
                        {
                            IdProducto = dto.IdProducto,
                            Nombre = dto.Nombre,
                            Descripcion = dto.Descripcion,
                            Precio = dto.Precio,
                            Disponible = dto.Disponible,
                            Unidades = dto.Unidades,
                            FechaRegistro = dto.FechaRegistro,
                            IdCategoria = dto.Categoria
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
            var categoria = CmbCategoriasFiltro.SelectedItem as CategoriaDTO;

            var lista = _listaCompletaCache.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(filtroTexto) && filtroTexto != "buscar producto...")
            {
                lista = lista.Where(p =>
                    p.Nombre.ToLower().Contains(filtroTexto) ||
                    p.Descripcion.ToLower().Contains(filtroTexto)
                );
            }

            if (categoria != null && categoria.id != 0)
            {
                lista = lista.Where(p =>
                    p.IdCategoria == categoria.id
                );
            }

            DataGridProductos.ItemsSource =
                new ObservableCollection<ProductoVistaAdminItem>(lista);
        }

        private void TxtBuscar_TextChanged(object sender, TextChangedEventArgs e) => FiltrarProductos();

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
            => FiltrarProductos();

        private void BtnModificar_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var producto = btn?.Tag as ProductoVistaAdminItem;

            if (producto == null)
            {
                MessageBox.Show("No se pudo obtener el producto.");
                return;
            }

            var ventana = Window.GetWindow(this) as wndMenuEmpleado;
            if (ventana != null)
            {
                var pagina = new wModificarProducto(producto);

                pagina.ProductoActualizado += OnProductoActualizado;

                ventana.fmPrincipal.Navigate(pagina);
            }
        }

        private async void OnProductoActualizado(int idProducto)
        {
            try
            {
                var resp = await _servicioProducto.ObtenerProductoPorIdAsync(idProducto, _token);

                if (resp.codigo != System.Net.HttpStatusCode.OK || resp.producto == null)
                    return;

                var dto = resp.producto;

                // Buscar el item existente
                var prod = ListaProductos.FirstOrDefault(p => p.IdProducto == idProducto);

                if (prod != null)
                {
                    prod.Nombre = dto.Nombre;
                    prod.Descripcion = dto.Descripcion;
                    prod.Precio = dto.Precio;
                    prod.Unidades = dto.Unidades;
                    prod.Disponible = dto.Disponible;
                    prod.IdCategoria = dto.Categoria;

                    // Notificar cambios
                    prod.OnPropertyChanged(nameof(prod.Nombre));
                    prod.OnPropertyChanged(nameof(prod.Descripcion));
                    prod.OnPropertyChanged(nameof(prod.Precio));
                    prod.OnPropertyChanged(nameof(prod.Unidades));
                    prod.OnPropertyChanged(nameof(prod.Disponible));
                    prod.OnPropertyChanged(nameof(prod.IdCategoria));
                }

                // Refrescar filtros
                FiltrarProductos();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al actualizar producto: " + ex.Message);
            }
        }

        private void BtnAgregarProducto_Click(object sender, RoutedEventArgs e)
        {
            var ventana = Window.GetWindow(this) as wndMenuEmpleado;

            if (ventana != null)
            {
                ventana.fmPrincipal.Navigate(
                    new sweet_temptation_clienteEscritorio.vista.producto.wRegistrarProducto()
                );
            }
        }


        private async void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var producto = btn?.Tag as ProductoVistaAdminItem;

            if (producto == null) return;

            var confirmar = MessageBox.Show(
                $"¿Desea eliminar '{producto.Nombre}'?",
                "Confirmar eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmar != MessageBoxResult.Yes)
                return;

            var respuesta = await _servicioProducto.EliminarProductoAsync(producto.IdProducto, _token);

            if (respuesta.codigo == System.Net.HttpStatusCode.OK)
            {
                ListaProductos.Remove(producto);
                _listaCompletaCache.Remove(producto);
                MessageBox.Show("Producto eliminado correctamente.");
                FiltrarProductos();
            }
            else
            {
                MessageBox.Show("Error al eliminar: " + respuesta.mensaje);
            }
        }
    }



    // Clase auxiliar
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
        public void OnPropertyChanged([CallerMemberName] string nombrePropiedad = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nombrePropiedad));
        }
    }
}


