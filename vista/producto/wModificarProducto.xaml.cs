using Newtonsoft.Json;
using sweet_temptation_clienteEscritorio.dto;
using sweet_temptation_clienteEscritorio.resources;
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

namespace sweet_temptation_clienteEscritorio.vista.producto
{
    public partial class wModificarProducto : Page
    {
        public event Action<int> ProductoActualizado;

        private ProductoVistaAdminItem _productoOriginal;
        private byte[] _imagenOriginal; 
        private bool _isEditing = false;
        private string _rutaNuevaImagen = string.Empty;

        private static readonly string API_PRODUCTOS = Constantes.URL + "producto";
        private static readonly string API_ARCHIVO = Constantes.URL + "archivo";
        private static readonly string API_CATEGORIAS = Constantes.URL + "categoria/todos";

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
            txtFechaRegistro.Text = _productoOriginal.FechaRegistro?.ToString("dd/MM/yyyy");

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

            btnModificarImagen.Visibility = isEditing ? Visibility.Visible : Visibility.Collapsed;
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
            if (cmbCategoria.SelectedValue == null) return Msg("Seleccione la categoría.");

            if (_imagenOriginal == null && string.IsNullOrEmpty(_rutaNuevaImagen))
                return Msg("Debe seleccionar una imagen para el producto que desea modificar.");


            return true;
        }

        private bool Msg(string m) { MessageBox.Show(m); return false; }

        private async void GuardarCambiosProducto()
        {
            if (!ValidarCampos(out decimal nuevoPrecio, out int nuevasUnidades)) return;

            var productoDatos = new
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
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

                using var contenidoMultipart = new MultipartFormDataContent();

                var json = JsonConvert.SerializeObject(productoDatos);
                var contenidoJson = new StringContent(json);
                contenidoJson.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                contenidoMultipart.Add(contenidoJson, "producto");

                if (!string.IsNullOrEmpty(_rutaNuevaImagen) && File.Exists(_rutaNuevaImagen))
                {
                    var bytesArchivo = await File.ReadAllBytesAsync(_rutaNuevaImagen);
                    var contenidoArchivo = new ByteArrayContent(bytesArchivo);

                    contenidoMultipart.Add(contenidoArchivo, "imagen", Path.GetFileName(_rutaNuevaImagen));
                }
                var response = await client.PutAsync($"{API_PRODUCTOS}/{_productoOriginal.IdProducto}", contenidoMultipart);

                if (!response.IsSuccessStatusCode)
                {
                    string error = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Error al actualizar producto: {response.StatusCode}\nDetalle: {error}", "Error de Actualización");
                    return;
                }

                _rutaNuevaImagen = string.Empty; 

                MessageBox.Show("Producto modificado correctamente.");

                ProductoActualizado?.Invoke(_productoOriginal.IdProducto);

                SetEditMode(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar los cambios: " + ex.Message);
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            if (_isEditing)
            {
                CargarDatosProductoEnPantalla();

                _rutaNuevaImagen = string.Empty; 

                if (_imagenOriginal != null)
                    imgProducto.Source = ConvertBytesToImage(_imagenOriginal);
                else
                    imgProducto.Source = null;

                SetEditMode(false);
            }

            MessageBox.Show("La modificacion del producto ha sido cancelada");

            //if (NavigationService.CanGoBack)
            //{
            //    NavigationService.GoBack();
            //}
        }

        private void btnModificarImagen_Click(object sender, RoutedEventArgs e)
        {

            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Archivos de Imagen|*.jpg;*.jpeg;*.png;*.bmp",
                Title = "Seleccionar Nueva Imagen para el Producto"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    _rutaNuevaImagen = openFileDialog.FileName;

                    imgProducto.Source = new BitmapImage(new Uri(_rutaNuevaImagen));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al cargar la imagen: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    _rutaNuevaImagen = string.Empty;
                }
            }
        }

        private void BtnRegresarClick(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }
    }
}