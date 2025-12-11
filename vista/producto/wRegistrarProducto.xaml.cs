using Microsoft.Win32;
using sweet_temptation_clienteEscritorio.dto;
using sweet_temptation_clienteEscritorio.servicios;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace sweet_temptation_clienteEscritorio.vista.producto
{
    public partial class wRegistrarProducto : Page
    {
        private readonly ProductoService _productoService;
        private readonly ArchivoService _archivoService;

        private string _token;

        private byte[] _imagenBytes = null;
        private string _rutaSeleccionadaImagen = null;

        public event Action<int> ProductoRegistrado;

        public wRegistrarProducto()
        {
            InitializeComponent();

            _productoService = new ProductoService(new HttpClient());
            _archivoService = new ArchivoService(new HttpClient());

            if (App.Current.Properties.Contains("Token"))
                _token = (string)App.Current.Properties["Token"];

            this.Loaded += async (s, e) => await CargarCategoriasAsync();
        }

        private async Task CargarCategoriasAsync()
        {
            var resp = await _productoService.ObtenerCategoriasAsync(_token);

            if (resp.codigo != HttpStatusCode.OK)
            {
                MessageBox.Show("Error al cargar categorías: " + resp.mensaje);
                return;
            }

            cmbCategoria.ItemsSource = resp.categorias;
            cmbCategoria.DisplayMemberPath = "nombre";
            cmbCategoria.SelectedValuePath = "id";
        }

        private void btnCargarImagen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new();
            dialog.Filter = "Imágenes|*.jpg;*.jpeg;*.png";

            if (dialog.ShowDialog() == true)
            {
                _rutaSeleccionadaImagen = dialog.FileName;
                _imagenBytes = File.ReadAllBytes(dialog.FileName);

                BitmapImage img = new();
                img.BeginInit();
                img.StreamSource = new MemoryStream(_imagenBytes);
                img.CacheOption = BitmapCacheOption.OnLoad;
                img.EndInit();
                img.Freeze();
                imgProducto.Source = img;
            }
        }

        private bool ValidarCampos(out decimal precio, out int unidades)
        {
            precio = 0; unidades = 0;

            if (string.IsNullOrWhiteSpace(txtNombreProducto.Text))
                return Error("El nombre es obligatorio.");

            if (string.IsNullOrWhiteSpace(txtDescripcion.Text))
                return Error("La descripción es obligatoria.");

            if (cmbCategoria.SelectedValue == null)
                return Error("Debe seleccionar una categoría.");

            if (!decimal.TryParse(txtPrecioUnitario.Text, out precio))
                return Error("Precio inválido.");

            if (!int.TryParse(txtUnidades.Text, out unidades))
                return Error("Unidades inválidas.");

            if (_imagenBytes == null)
                return Error("Debe cargar una imagen.");

            return true;
        }

        private bool Error(string msg)
        {
            MessageBox.Show(msg, "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        private async void btnRegistrar_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarCampos(out decimal precio, out int unidades))
                return;

            // Obtener extensión real
            string extension = Path.GetExtension(_rutaSeleccionadaImagen)?
                                .Replace(".", "").ToLower();

            string[] permitidas = { "jpg", "jpeg", "png" };
            if (!permitidas.Contains(extension))
            {
                MessageBox.Show("Formato no permitido.");
                return;
            }

            // Crear producto
            var nuevoProducto = new ProductoDTO
            {
                Nombre = txtNombreProducto.Text,
                Descripcion = txtDescripcion.Text,
                Precio = precio,
                Unidades = unidades,
                Disponible = true,
                Categoria = (int)cmbCategoria.SelectedValue
            };

            var resp = await _productoService.CrearProductoAsync(nuevoProducto, _token);

            if (resp.codigo != HttpStatusCode.OK &&
                resp.codigo != HttpStatusCode.Created)
            {
                MessageBox.Show("ERROR al registrar producto:\n" + resp.mensaje);
                return;
            }

            int idProducto = resp.idProducto;

            // guardar archivo
            var archivo = new ArchivoDTO
            {
                extension = extension,
                datos = _imagenBytes
            };

            var respArchivo = await _archivoService.GuardarArchivoAsync(archivo, _token);

            if (respArchivo.codigo != HttpStatusCode.OK &&
                respArchivo.codigo != HttpStatusCode.Created)
            {
                MessageBox.Show($"ERROR AL GUARDAR ARCHIVO:\nCódigo: {respArchivo.codigo}\nMensaje: {respArchivo.mensaje}");
                return;
            }

            int idArchivo = respArchivo.idArchivo;

            var respAsociar = await _archivoService.AsociarArchivoAsync(idArchivo, idProducto, _token);

            if (respAsociar.codigo != HttpStatusCode.OK &&
                respAsociar.codigo != HttpStatusCode.Created)
            {
                MessageBox.Show("Imagen guardada, pero NO se pudo asociar:\n" + respAsociar.mensaje);
                return;
            }

            MessageBox.Show("Producto registrado correctamente.");

            ProductoRegistrado?.Invoke(idProducto);

            var ventana = Window.GetWindow(this) as wndMenuEmpleado;
            ventana.fmPrincipal.Navigate(new wAdministrarProductos());
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            var ventana = Window.GetWindow(this) as wndMenuEmpleado;
            ventana.fmPrincipal.Navigate(new wAdministrarProductos());
        }

        private void BtnClickRegresar(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
        }
    }
}
