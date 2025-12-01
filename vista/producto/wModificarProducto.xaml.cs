using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace sweet_temptation_clienteEscritorio.vista.producto
{
    /// <summary>
    /// Lógica de interacción para wModificarProducto.xaml
    /// </summary>
    public partial class wModificarProducto : Page
    {
        private bool _isEditing = false;
        private Producto _productoActual;

        public class CategoriaItem
        {
            public int Id { get; set; }
            public string Nombre { get; set; }
            public override string ToString() => Nombre; 
        }


        public wModificarProducto()
        {
            InitializeComponent();
            CargarDatosProductoEjemplo();
            SetEditMode(false);
        }

        private void CargarDatosProductoEjemplo()
        {
            // Ejemplo de producto (quitar despues)
            _productoActual = new Producto
            {
                IdProducto = 1,
                Nombre = "Torta Selva Negra",
                Descripcion = "Deliciosa torta de chocolate, cerezas y crema chantilly.",
                Categoria = 101, 
                Precio = 35.00m,
                Unidades = 15,
              
            };
            cmbCategoria.Items.Clear();
            var categoria1 = new CategoriaItem { Id = 101, Nombre = "Pasteles Clásicos" };
            var categoria2 = new CategoriaItem { Id = 102, Nombre = "Postres Fríos" };
            cmbCategoria.Items.Add(categoria1);
            cmbCategoria.Items.Add(categoria2);

            foreach (CategoriaItem item in cmbCategoria.Items)
            {
                if (item.Id == _productoActual.Categoria)
                {
                    cmbCategoria.SelectedItem = item;
                    break;
                }
            }

            txtNombreProducto.Text = _productoActual.Nombre;
            txtDescripcion.Text = _productoActual.Descripcion;
            txtPrecioUnitario.Text = _productoActual.Precio.ToString("N2");
            txtUnidades.Text = _productoActual.Unidades.ToString();

            try
            {
                imgProducto.Source = new BitmapImage(new Uri("https://placehold.co/350x350/E0E0E0/333333?text=Placeholder+Producto", UriKind.Absolute));
            }
            catch { /* Manejar error de carga de imagen */ }
        }

        private void SetEditMode(bool isEditing)
        {
            _isEditing = isEditing;

            txtNombreProducto.IsReadOnly = !isEditing;
            txtDescripcion.IsReadOnly = !isEditing;
            txtPrecioUnitario.IsReadOnly = !isEditing;
            txtUnidades.IsReadOnly = !isEditing;

            cmbCategoria.IsEnabled = isEditing;
            btnCargarImagen.IsEnabled = isEditing;

            btnAccion.Content = isEditing ? "Guardar Cambios" : "Modificar";
        }

        private void btnAccion_Click(object sender, RoutedEventArgs e)
        {
            if (!_isEditing)
            {
                SetEditMode(true);
            }
            else
            {
                GuardarCambios();
                SetEditMode(false);
            }
        }

        private void GuardarCambios()
        {
            // TODO - obtener info
            string nuevoNombre = txtNombreProducto.Text;
            string nuevaDescripcion = txtDescripcion.Text;

            // TODO validaciones
            if (!decimal.TryParse(txtPrecioUnitario.Text, out decimal nuevoPrecio) ||
                !int.TryParse(txtUnidades.Text, out int nuevasUnidades))
            {
                MessageBox.Show("Por favor, introduce valores numéricos válidos para Precio y Unidades.", "Error de Validación");
                return;
            }

            CategoriaItem categoriaSeleccionada = cmbCategoria.SelectedItem as CategoriaItem;
            if (categoriaSeleccionada == null)
            {
                MessageBox.Show("Por favor, selecciona una Categoría válida.", "Error de Validación");
                return;
            }

            _productoActual.Nombre = nuevoNombre;
            _productoActual.Descripcion = nuevaDescripcion;
            _productoActual.Precio = nuevoPrecio;
            _productoActual.Unidades = nuevasUnidades;
            _productoActual.Categoria = categoriaSeleccionada.Id;

            MessageBox.Show($"Producto '{_productoActual.Nombre}' (ID: {_productoActual.IdProducto}) modificado y guardado correctamente." +
                            $"\nNueva Categoría ID: {_productoActual.Categoria}", "Modificación Exitosa");
        }

        private void btnCargarImagen_Click(object sender, RoutedEventArgs e)
        {
            // Lógica para abrir OpenFileDialog y seleccionar una nueva imagen
            MessageBox.Show("Seleccionar archivo de imagen...");
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            if (_isEditing)
            {
                // Si estaba editando, simplemente recargar los datos originales y salir del modo edición
                CargarDatosProductoEjemplo();
                SetEditMode(false);
                MessageBox.Show("Cambios descartados.");
            }
            else
            {
                MessageBox.Show("Navegar a la página anterior (Listado de productos).");
               
            }
        }
    }
}
