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

namespace sweet_temptation_clienteEscritorio.vista.producto
{
    public partial class wAdministrarProductos : Page
    {
        private ProductoService _servicioProducto;
        private string _token;

        public ObservableCollection<ProductoVistaAdminItem> ListaProductos { get; set; }

        private List<ProductoVistaAdminItem> _listaCompletaCache;

        public wAdministrarProductos()
        {
            InitializeComponent();

            var http = new HttpClient();
            _servicioProducto = new ProductoService(http);

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
                        new CategoriaDTO { id = 0, nombre = "Todas las Categorías" }
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

        private void CmbCategoriasFiltro_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => FiltrarProductos();

        private void TxtBuscar_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TxtBuscar.Text == "Buscar producto...") TxtBuscar.Text = "";
        }

        private void TxtBuscar_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtBuscar.Text)) TxtBuscar.Text = "Buscar producto...";
        }

        private void BtnModificar_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var producto = btn?.Tag as ProductoVistaAdminItem;

            if (producto == null) return;

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
                var prod = ListaProductos.FirstOrDefault(p => p.IdProducto == idProducto);

                if (prod != null)
                {
                    prod.Nombre = dto.Nombre;
                    prod.Descripcion = dto.Descripcion;
                    prod.Precio = dto.Precio;
                    prod.Unidades = dto.Unidades;
                    prod.Disponible = dto.Disponible;
                    prod.IdCategoria = dto.Categoria;

                    prod.RefrescarPropiedades();
                }
                FiltrarProductos();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al actualizar lista: " + ex.Message);
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

            if (confirmar != MessageBoxResult.Yes) return;

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

        public event PropertyChangedEventHandler PropertyChanged;

        public void RefrescarPropiedades()
        {
            OnPropertyChanged(nameof(Nombre));
            OnPropertyChanged(nameof(Descripcion));
            OnPropertyChanged(nameof(Precio));
            OnPropertyChanged(nameof(Unidades));
            OnPropertyChanged(nameof(Disponible));
            OnPropertyChanged(nameof(TextoDisponibilidad));
        }

        protected void OnPropertyChanged([CallerMemberName] string nombrePropiedad = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nombrePropiedad));
        }
    }
}

