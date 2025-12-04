using Microsoft.Win32;
using sweet_temptation_clienteEscritorio.dto;
using sweet_temptation_clienteEscritorio.servicios;
using System;
using System.IO;
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
        private ProductoService _servicioProducto;
        private ArchivoService _servicioArchivo;

        private string _token;
        private byte[] _imagenBytes = null;

        public wRegistrarProducto()
        {
            InitializeComponent();

            _servicioProducto = new ProductoService(new HttpClient());
            _servicioArchivo = new ArchivoService(new HttpClient());

            if (App.Current.Properties.Contains("Token"))
                _token = (string)App.Current.Properties["Token"];

            Loaded += async (s, e) =>
            {
                await CargarCategoriasAsync();
            };
        }

        private async Task CargarCategoriasAsync()
        {
            try
            {
                var respuesta = await _servicioProducto.ObtenerCategoriasAsync(_token);

                if (respuesta.codigo == System.Net.HttpStatusCode.OK && respuesta.categorias != null)
                {
                    cmbCategoria.ItemsSource = respuesta.categorias;
                    cmbCategoria.DisplayMemberPath = "nombre";
                    cmbCategoria.SelectedValuePath = "id";
                    cmbCategoria.SelectedIndex = 0;
                }
                else
                {
                    MessageBox.Show("No se pudieron cargar las categorías.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar categorías: " + ex.Message);
            }
        }

        private void btnCargarImagen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Imágenes (*.jpg;*.png)|*.jpg;*.png";

            if (dlg.ShowDialog() == true)
            {
                imgProducto.Source = new BitmapImage(new Uri(dlg.FileName));
                _imagenBytes = File.ReadAllBytes(dlg.FileName);

                MessageBox.Show("Imagen cargada correctamente.");
            }
        }

        private bool ValidarFormulario()
        {
            if (string.IsNullOrWhiteSpace(txtNombreProducto.Text) ||
                string.IsNullOrWhiteSpace(txtDescripcion.Text) ||
                string.IsNullOrWhiteSpace(txtPrecioUnitario.Text) ||
                string.IsNullOrWhiteSpace(txtUnidades.Text) ||
                cmbCategoria.SelectedValue == null)
            {
                MessageBox.Show("Completa todos los campos.");
                return false;
            }

            if (!decimal.TryParse(txtPrecioUnitario.Text, out _) ||
                !int.TryParse(txtUnidades.Text, out _))
            {
                MessageBox.Show("Precio y unidades deben ser numéricos.");
                return false;
            }

            if (_imagenBytes == null)
            {
                MessageBox.Show("Debes seleccionar una imagen para el producto.");
                return false;
            }

            return true;
        }

        private async void btnRegistrar_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarFormulario())
                return;

            try
            {
                ProductoDTO nuevo = new ProductoDTO
                {
                    Nombre = txtNombreProducto.Text,
                    Descripcion = txtDescripcion.Text,
                    Precio = decimal.Parse(txtPrecioUnitario.Text),
                    Unidades = int.Parse(txtUnidades.Text),
                    Disponible = true,
                    Categoria = (int)cmbCategoria.SelectedValue
                };

                var respProducto = await _servicioProducto.CrearProductoAsync(nuevo, _token);

                if (respProducto.codigo != HttpStatusCode.Created &&
                    respProducto.codigo != HttpStatusCode.OK)
                {
                    MessageBox.Show("Error al registrar producto: " + respProducto.mensaje);
                    return;
                }

                int idProducto = respProducto.idProducto;

                ArchivoDTO archivo = new ArchivoDTO
                {
                    extension = "jpg",
                    datos = _imagenBytes
                };

                var respArchivo = await _servicioArchivo.GuardarArchivoAsync(archivo, _token);

                if (respArchivo.codigo != System.Net.HttpStatusCode.OK &&
                    respArchivo.codigo != System.Net.HttpStatusCode.Created)
                {
                    MessageBox.Show("Producto creado, pero la imagen no se guardó.");
                    return;
                }

                int idArchivo = respArchivo.idArchivo;

                var respAsociar = await _servicioArchivo.AsociarArchivoAsync(idArchivo, idProducto, _token);

                if (respAsociar.codigo != System.Net.HttpStatusCode.OK &&
                    respAsociar.codigo != System.Net.HttpStatusCode.Created)
                {
                    MessageBox.Show("Producto creado, pero la imagen NO se asoció.");
                    return;
                }

                MessageBox.Show("Producto registrado con éxito y con imagen asociada.");

                LimpiarFormulario();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al registrar: " + ex.Message);
            }
        }

        private void LimpiarFormulario()
        {
            txtNombreProducto.Clear();
            txtDescripcion.Clear();
            txtPrecioUnitario.Clear();
            txtUnidades.Clear();
            cmbCategoria.SelectedIndex = 0;
            imgProducto.Source = null;
            _imagenBytes = null;
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            LimpiarFormulario();
        }
    }
}
