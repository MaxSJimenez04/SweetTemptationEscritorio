using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using sweet_temptation_clienteEscritorio.dto;

namespace sweet_temptation_clienteEscritorio.vista.producto
{
    public partial class wModificarProducto : Page
    {
        public event Action<int> ProductoActualizado;

        private ProductoVistaAdminItem _productoOriginal;
        private byte[] _imagenOriginal; 


        private bool _isEditing = false;

        private static readonly string API_PRODUCTOS = "http://localhost:8080/producto";
        private static readonly string API_ARCHIVO = "http://localhost:8080/archivo";
        private static readonly string API_CATEGORIAS = "http://localhost:8080/categoria/todos";

        private readonly string _token;

        public wModificarProducto(ProductoVistaAdminItem producto)
        {
            InitializeComponent();

            _productoOriginal = producto;

            if (App.Current.Properties.Contains("Token"))
                _token = (string)App.Current.Properties["Token"];

            Loaded += async (s, e) =>
            {
                await CargarCategoriasAsync();
                CargarDatosProductoEnPantalla();
                await CargarImagenProductoDesdeAPI();
            };

            SetEditMode(false);
        }

        private void CargarDatosProductoEnPantalla()
        {
            txtNombreProducto.Text = _productoOriginal.Nombre;
            txtDescripcion.Text = _productoOriginal.Descripcion;
            txtPrecioUnitario.Text = _productoOriginal.Precio.ToString("N2");
            txtUnidades.Text = _productoOriginal.Unidades.ToString();
            txtFechaRegistro.Text = _productoOriginal.FechaRegistro?.ToString("dd/MM/yyyy HH:mm");

            cmbDisponible.SelectedIndex = _productoOriginal.Disponible ? 0 : 1;
        }

        private async Task CargarCategoriasAsync()
        {
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                  new AuthenticationHeaderValue("Bearer", _token);

                var response = await client.GetAsync(API_CATEGORIAS);

                if (!response.IsSuccessStatusCode)
                    throw new Exception("Error API Categorías");

                var json = await response.Content.ReadAsStringAsync();
                var categorias = JsonConvert.DeserializeObject<List<CategoriaDTO>>(json);

                cmbCategoria.ItemsSource = categorias;
                cmbCategoria.DisplayMemberPath = "nombre";
                cmbCategoria.SelectedValuePath = "id";

                cmbCategoria.SelectedValue = _productoOriginal.IdCategoria;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar categorías: " + ex.Message);
            }
        }

        private async Task CargarImagenProductoDesdeAPI()
        {
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                  new AuthenticationHeaderValue("Bearer", _token);

                string urlDetalles = $"{API_ARCHIVO}/detalle?idProducto={_productoOriginal.IdProducto}";
                var respDetalles = await client.GetAsync(urlDetalles);

                if (!respDetalles.IsSuccessStatusCode)
                {
                    imgProducto.Source = null;
                    return;
                }

                var jsonDetalles = await respDetalles.Content.ReadAsStringAsync();
                var detalles = JsonConvert.DeserializeObject<DetallesArchivoDTO>(jsonDetalles);

                if (detalles == null || string.IsNullOrWhiteSpace(detalles.ruta))
                {
                    imgProducto.Source = null;
                    return;
                }

                string idArchivo = detalles.ruta;
                if (idArchivo.Contains("/")) idArchivo = idArchivo.Substring(idArchivo.LastIndexOf('/') + 1);
                if (idArchivo.Contains("\\")) idArchivo = idArchivo.Substring(idArchivo.LastIndexOf('\\') + 1);

                string urlImagen = $"{API_ARCHIVO}/{idArchivo}";

                var respArchivo = await client.GetAsync(urlImagen);

                if (!respArchivo.IsSuccessStatusCode)
                {
                    imgProducto.Source = null;
                    return;
                }

                var jsonArchivo = await respArchivo.Content.ReadAsStringAsync();
                var archivo = JsonConvert.DeserializeObject<ArchivoDTO>(jsonArchivo);

                if (archivo != null && archivo.datos != null)
                {
                    _imagenOriginal = archivo.datos;
                    imgProducto.Source = ConvertBytesToImage(_imagenOriginal);
                }
            }
            catch
            {
                imgProducto.Source = null;
                _imagenOriginal = null;
            }
        }

        private BitmapImage ConvertBytesToImage(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0) return null;
            using MemoryStream ms = new(bytes);
            BitmapImage img = new();
            img.BeginInit();
            img.StreamSource = ms;
            img.CacheOption = BitmapCacheOption.OnLoad;
            img.EndInit();
            img.Freeze();
            return img;
        }

        private void SetEditMode(bool isEditing)
        {
            _isEditing = isEditing;

            txtNombreProducto.IsReadOnly = !isEditing;
            txtDescripcion.IsReadOnly = !isEditing;
            txtPrecioUnitario.IsReadOnly = !isEditing;
            txtUnidades.IsReadOnly = !isEditing;

            cmbCategoria.IsEnabled = isEditing;
            cmbDisponible.IsEnabled = isEditing;

            btnAccion.Content = isEditing ? "Guardar Cambios" : "Modificar";
        }

        private void btnAccion_Click(object sender, RoutedEventArgs e)
        {
            if (!_isEditing)
                SetEditMode(true);
            else
                GuardarCambiosProducto();
        }

        private bool ValidarCampos(out decimal precio, out int unidades)
        {
            precio = 0; unidades = 0;

            if (txtNombreProducto.Text.Length < 3) return Msg("Nombre muy corto.");
            if (txtDescripcion.Text.Length < 10) return Msg("Descripción muy corta.");
            if (!decimal.TryParse(txtPrecioUnitario.Text, out precio) || precio <= 0) return Msg("Precio inválido.");
            if (!int.TryParse(txtUnidades.Text, out unidades) || unidades < 0) return Msg("Unidades inválidas.");
            if (cmbCategoria.SelectedValue == null) return Msg("Seleccione categoría.");

            if (_imagenOriginal == null)
                return Msg("El producto debe tener una imagen previa cargada.");

            return true;
        }

        private bool Msg(string m) { MessageBox.Show(m); return false; }

        private async void GuardarCambiosProducto()
        {
            if (!ValidarCampos(out decimal nuevoPrecio, out int nuevasUnidades)) return;

            var productoActualizado = new
            {
                nombre = txtNombreProducto.Text,
                descripcion = txtDescripcion.Text,
                precio = nuevoPrecio,
                disponible = (cmbDisponible.SelectedIndex == 0),
                unidades = nuevasUnidades,
                categoria = (int)cmbCategoria.SelectedValue
            };

            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                  new AuthenticationHeaderValue("Bearer", _token);

                var json = JsonConvert.SerializeObject(productoActualizado);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PutAsync($"{API_PRODUCTOS}/{_productoOriginal.IdProducto}", content);

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Error al actualizar datos del producto.");
                    return;
                }


                MessageBox.Show("Producto modificado correctamente.");

                ProductoActualizado?.Invoke(_productoOriginal.IdProducto);

                SetEditMode(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            if (_isEditing)
            {
                CargarDatosProductoEnPantalla();
                if (_imagenOriginal != null)
                    imgProducto.Source = ConvertBytesToImage(_imagenOriginal);
                else
                    imgProducto.Source = null;


                SetEditMode(false);
            }
            else
            {
                if (NavigationService.CanGoBack)
                    NavigationService.GoBack();
            }
        }
    }
}