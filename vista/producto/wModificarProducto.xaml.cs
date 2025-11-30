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

        // Clase auxiliar para manejar la visualización en el ComboBox (Nombre)
        // y el valor real que se debe guardar (Id)
        public class CategoriaItem
        {
            public int Id { get; set; }
            public string Nombre { get; set; }
            public override string ToString() => Nombre; // Muestra el nombre en el ComboBox
        }


        public wModificarProducto()
        {
            InitializeComponent();
            // Carga los datos del producto al inicializar
            CargarDatosProductoEjemplo();
            // Establece el estado inicial de la interfaz (Solo lectura)
            SetEditMode(false);
        }

        // Simula la carga de datos de un producto desde tu lógica de negocio/base de datos
        private void CargarDatosProductoEjemplo()
        {
            // Simulación de un producto cargado
            _productoActual = new Producto
            {
                IdProducto = 1,
                Nombre = "Torta Selva Negra",
                Descripcion = "Deliciosa torta de chocolate, cerezas y crema chantilly.",
                Categoria = 101, // Usamos el ID de la categoría
                Precio = 35.00m,
                Unidades = 15,
                //ImagenPath = "ruta/a/tu/imagen.jpg" // Usa una ruta de imagen válida en tu proyecto
            };

            // 1. Poblamos el ComboBox con objetos CategoriaItem
            cmbCategoria.Items.Clear();
            var categoria1 = new CategoriaItem { Id = 101, Nombre = "Pasteles Clásicos" };
            var categoria2 = new CategoriaItem { Id = 102, Nombre = "Postres Fríos" };
            cmbCategoria.Items.Add(categoria1);
            cmbCategoria.Items.Add(categoria2);

            // 2. Buscamos el objeto CategoriaItem que corresponde al ID del producto
            // y lo seleccionamos para la visualización inicial
            foreach (CategoriaItem item in cmbCategoria.Items)
            {
                if (item.Id == _productoActual.Categoria)
                {
                    cmbCategoria.SelectedItem = item;
                    break;
                }
            }

            // Rellena los campos con los datos
            txtNombreProducto.Text = _productoActual.Nombre;
            txtDescripcion.Text = _productoActual.Descripcion;
            txtPrecioUnitario.Text = _productoActual.Precio.ToString("N2");
            txtUnidades.Text = _productoActual.Unidades.ToString();

            // Simular carga de imagen (reemplaza con tu lógica real de carga de imagen)
            try
            {
                // Reemplaza esto con tu código real para cargar la imagen
                // imgProducto.Source = new BitmapImage(new Uri(_productoActual.ImagenPath, UriKind.RelativeOrAbsolute));
                // Usaremos un placeholder para que no falle al previsualizar
                imgProducto.Source = new BitmapImage(new Uri("https://placehold.co/350x350/E0E0E0/333333?text=Placeholder+Producto", UriKind.Absolute));
            }
            catch { /* Manejar error de carga de imagen */ }
        }

        // Método para alternar entre modo visualización y modo edición
        private void SetEditMode(bool isEditing)
        {
            _isEditing = isEditing;

            // Habilita/Deshabilita TextBoxes
            txtNombreProducto.IsReadOnly = !isEditing;
            txtDescripcion.IsReadOnly = !isEditing;
            txtPrecioUnitario.IsReadOnly = !isEditing;
            txtUnidades.IsReadOnly = !isEditing;

            // Habilita/Deshabilita ComboBox y Botón de Imagen
            cmbCategoria.IsEnabled = isEditing;
            btnCargarImagen.IsEnabled = isEditing;

            // Cambia el texto del botón de acción
            btnAccion.Content = isEditing ? "Guardar Cambios" : "Modificar";
        }

        // Manejador del botón "Modificar" / "Guardar Cambios"
        private void btnAccion_Click(object sender, RoutedEventArgs e)
        {
            if (!_isEditing)
            {
                // Modo Visualización -> Cambiar a Modo Edición
                SetEditMode(true);
            }
            else
            {
                // Modo Edición -> Guardar Cambios y volver a Modo Visualización
                GuardarCambios();
                SetEditMode(false);
            }
        }

        private void GuardarCambios()
        {
            // 1. Obtener los nuevos valores de los campos
            string nuevoNombre = txtNombreProducto.Text;
            string nuevaDescripcion = txtDescripcion.Text;

            // Aquí iría tu lógica de validación
            if (!decimal.TryParse(txtPrecioUnitario.Text, out decimal nuevoPrecio) ||
                !int.TryParse(txtUnidades.Text, out int nuevasUnidades))
            {
                MessageBox.Show("Por favor, introduce valores numéricos válidos para Precio y Unidades.", "Error de Validación");
                return;
            }

            // 2. Obtener el ID de la Categoría seleccionada
            CategoriaItem categoriaSeleccionada = cmbCategoria.SelectedItem as CategoriaItem;
            if (categoriaSeleccionada == null)
            {
                MessageBox.Show("Por favor, selecciona una Categoría válida.", "Error de Validación");
                return;
            }

            // 3. Actualizar el objeto producto (o llamar a tu servicio/método de B.D.)
            _productoActual.Nombre = nuevoNombre;
            _productoActual.Descripcion = nuevaDescripcion;
            _productoActual.Precio = nuevoPrecio;
            _productoActual.Unidades = nuevasUnidades;
            // Corregido: Asignamos el Id (int) de la categoría
            _productoActual.Categoria = categoriaSeleccionada.Id;

            // 4. Llamar a tu lógica de guardado en la base de datos (Ej: productoService.Update(_productoActual))
            MessageBox.Show($"Producto '{_productoActual.Nombre}' (ID: {_productoActual.IdProducto}) modificado y guardado correctamente." +
                            $"\nNueva Categoría ID: {_productoActual.Categoria}", "Modificación Exitosa");
        }


        // Manejadores de botones (ejemplos)
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
                // Si estaba solo visualizando, navegar a la página anterior o cerrar la vista
                MessageBox.Show("Navegar a la página anterior (Listado de productos).");
                // NavigationService?.GoBack(); // Descomentar si usas NavigationService
            }
        }
    }
}
