using Microsoft.Win32;
using Newtonsoft.Json;
using sweet_temptation_clienteEscritorio.dto;
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
        // 🔥 Evento que notificará a la ventana anterior
        public event Action<int> ProductoActualizado;

        private ProductoVistaAdminItem _productoOriginal;
        private byte[] _imagenOriginal;
        private byte[] _nuevaImagenBytes = null;
        private string _extensionNuevaImagen = null;
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

        // ============================================================
        //     CARGA DATOS DEL PRODUCTO EN CONTROLES
        // ============================================================
        private void CargarDatosProductoEnPantalla()
        {
            txtNombreProducto.Text = _productoOriginal.Nombre;
            txtDescripcion.Text = _productoOriginal.Descripcion;
            txtPrecioUnitario.Text = _productoOriginal.Precio.ToString("N2");
            txtUnidades.Text = _productoOriginal.Unidades.ToString();

            txtFechaRegistro.Text = _productoOriginal.FechaRegistro?.ToString("dd/MM/yyyy HH:mm");

            cmbDisponible.SelectedIndex = _productoOriginal.Disponible ? 0 : 1;
        }

        // ============================================================
        //   OBTENER CATEGORÍAS DESDE API (CON TOKEN)
        // ============================================================
        private async Task CargarCategoriasAsync()
        {
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _token);

                var response = await client.GetAsync(API_CATEGORIAS);

                if (!response.IsSuccessStatusCode)
                    throw new Exception("No se pudieron obtener las categorías.");

                var json = await response.Content.ReadAsStringAsync();
                var categorias = JsonConvert.DeserializeObject<List<CategoriaDTO>>(json);

                cmbCategoria.ItemsSource = categorias;
                cmbCategoria.DisplayMemberPath = "nombre";
                cmbCategoria.SelectedValuePath = "id";

                // Seleccionar categoría actual
                cmbCategoria.SelectedValue = _productoOriginal.IdCategoria;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar categorías: " + ex.Message);
            }
        }

        // ============================================================
        //   CARGAR IMAGEN DESDE API (CON TOKEN)
        // ============================================================
        private async Task CargarImagenProductoDesdeAPI()
        {
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _token);

                var respDetalles = await client.GetAsync($"{API_ARCHIVO}/?idProducto={_productoOriginal.IdProducto}");

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

                var reqArchivo = new HttpRequestMessage(HttpMethod.Get, detalles.ruta);
                reqArchivo.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

                var respArchivo = await client.SendAsync(reqArchivo);

                if (!respArchivo.IsSuccessStatusCode)
                {
                    imgProducto.Source = null;
                    return;
                }

                var jsonArchivo = await respArchivo.Content.ReadAsStringAsync();
                var archivo = JsonConvert.DeserializeObject<ArchivoDTO>(jsonArchivo);

                _imagenOriginal = archivo.datos;
                imgProducto.Source = ConvertBytesToImage(_imagenOriginal);
            }
            catch
            {
                imgProducto.Source = null;
                _imagenOriginal = null;
            }
        }

        private BitmapImage ConvertBytesToImage(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                return null;

            using MemoryStream ms = new(bytes);
            BitmapImage img = new();
            img.BeginInit();
            img.StreamSource = ms;
            img.CacheOption = BitmapCacheOption.OnLoad;
            img.EndInit();
            img.Freeze();
            return img;
        }

        // ============================================================
        //   MODO EDICIÓN
        // ============================================================
        private void SetEditMode(bool isEditing)
        {
            _isEditing = isEditing;

            txtNombreProducto.IsReadOnly = !isEditing;
            txtDescripcion.IsReadOnly = !isEditing;
            txtPrecioUnitario.IsReadOnly = !isEditing;
            txtUnidades.IsReadOnly = !isEditing;

            cmbCategoria.IsEnabled = isEditing;
            cmbDisponible.IsEnabled = isEditing;
            btnCargarImagen.IsEnabled = isEditing;

            btnAccion.Content = isEditing ? "Guardar Cambios" : "Modificar";
        }

        // ============================================================
        //   CARGAR NUEVA IMAGEN
        // ============================================================
        private void btnCargarImagen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new();
            dialog.Filter = "Imágenes|*.jpg;*.png;*.jpeg";

            if (dialog.ShowDialog() == true)
            {
                _nuevaImagenBytes = File.ReadAllBytes(dialog.FileName);

                _extensionNuevaImagen = Path.GetExtension(dialog.FileName)
                    .Replace(".", "")
                    .ToLower();

                imgProducto.Source = ConvertBytesToImage(_nuevaImagenBytes);
            }
        }

        // ============================================================
        //   BOTÓN PRINCIPAL
        // ============================================================
        private void btnAccion_Click(object sender, RoutedEventArgs e)
        {
            if (!_isEditing)
                SetEditMode(true);
            else
                GuardarCambiosProducto();
        }

        // ============================================================
        //   VALIDACIONES
        // ============================================================
        private bool ValidarCampos(out decimal precio, out int unidades)
        {
            precio = 0; unidades = 0;

            if (txtNombreProducto.Text.Length < 3)
                return Msg("El nombre debe tener al menos 3 caracteres.");

            if (txtDescripcion.Text.Length < 10)
                return Msg("La descripción debe tener mínimo 10 caracteres.");

            if (!decimal.TryParse(txtPrecioUnitario.Text, out precio) || precio <= 0)
                return Msg("Precio inválido.");

            if (!int.TryParse(txtUnidades.Text, out unidades) || unidades < 0)
                return Msg("Unidades inválidas.");

            if (cmbCategoria.SelectedValue == null)
                return Msg("Debe seleccionar una categoría.");

            if (_imagenOriginal == null && _nuevaImagenBytes == null)
                return Msg("Debe haber una imagen.");

            return true;
        }

        private bool Msg(string m)
        {
            MessageBox.Show(m);
            return false;
        }

        // ============================================================
        //   GUARDAR CAMBIOS DEL PRODUCTO (CON TOKEN)
        // ============================================================
        private async void GuardarCambiosProducto()
        {
            if (!ValidarCampos(out decimal nuevoPrecio, out int nuevasUnidades))
                return;

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
                    MessageBox.Show("Error al actualizar producto.");
                    return;
                }

                // Si hay nueva imagen → guardar y asociar
                if (_nuevaImagenBytes != null)
                    await GuardarYAsociarNuevaImagen();

                MessageBox.Show("Producto modificado correctamente.");

                // 🔥 Avisar al listado que este producto cambió
                ProductoActualizado?.Invoke(_productoOriginal.IdProducto);

                SetEditMode(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error inesperado: " + ex.Message);
            }
        }

        // ============================================================
        //   GUARDAR Y ASOCIAR NUEVA IMAGEN (CON TOKEN)
        // ============================================================
        private async Task GuardarYAsociarNuevaImagen()
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _token);

            var archivo = new
            {
                extension = _extensionNuevaImagen ?? "jpg",
                datos = _nuevaImagenBytes
            };

            var json = JsonConvert.SerializeObject(archivo);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var resp = await client.PostAsync($"{API_ARCHIVO}/", content);
            string idArchivoStr = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode || !int.TryParse(idArchivoStr, out int idArchivo))
            {
                MessageBox.Show("Error al guardar la imagen.");
                return;
            }

            await client.PostAsync(
                $"{API_ARCHIVO}/asociar?idArchivo={idArchivo}&idProducto={_productoOriginal.IdProducto}",
                null);
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            if (_isEditing)
            {
                CargarDatosProductoEnPantalla();
                imgProducto.Source = ConvertBytesToImage(_imagenOriginal);

                _nuevaImagenBytes = null;
                _extensionNuevaImagen = null;

                SetEditMode(false);
            }
            else
            {
                MessageBox.Show("Volver al listado de productos.");
            }
        }
    }
}

