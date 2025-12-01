using Microsoft.Win32;
using sweet_temptation_clienteEscritorio.model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
// Asegúrate de que los namespaces de tus modelos sean accesibles
// Por ejemplo: using sweet_temptation_clienteEscritorio.Modelo;

namespace sweet_temptation_clienteEscritorio.vista.producto
{
    /// <summary>
    /// Lógica de interacción para wRegistrarProducto.xaml
    /// </summary>
    public partial class wRegistrarProducto : Page
    {
        // CONSTANTE DE LA URL BASE DE TU API PARA REGISTRAR PRODUCTOS
        // AJUSTADO: Usando /producto/nuevo según el ProductoController de Spring Boot, PUERTO CAMBIADO A 8080.
        // *** IMPORTANTE: AJUSTA LA BASE_URL (localhost:8080) si tu servidor usa otro puerto/dominio ***
        private const string API_PRODUCTOS_URL = "http://localhost:8080/producto/nuevo";

        // NUEVA CONSTANTE PARA OBTENER CATEGORÍAS
        // AJUSTADO: Se asume el patrón /categoria/todos para obtener todas las categorías, PUERTO CAMBIADO A 8080.
        // *** IMPORTANTE: Confirma que este es el endpoint correcto en tu CategoriaController ***
        private const string API_CATEGORIAS_URL = "http://localhost:8080/categoria/todos";

        // NUEVA CONSTANTE PARA CONSULTAR UN PRODUCTO POR NOMBRE (ASUMIDO)
        // Se asume un endpoint en tu API que permite buscar productos por nombre, PUERTO CAMBIADO A 8080.
        // *** AJUSTAR: Reemplaza o implementa este endpoint en tu servidor (ej: /producto/nombre/{nombre}) ***
        private const string API_PRODUCTO_POR_NOMBRE_URL = "http://localhost:8080/producto/nombre";

        private HttpClient _httpClient;
        private string _imagenBase64 = string.Empty; // Para almacenar la imagen codificada

        public wRegistrarProducto()
        {
            InitializeComponent();
            _httpClient = new HttpClient();
            // Iniciar la carga de categorías al inicializar el componente
            _ = CargarCategoriasAsync();
        }

        // --- Manejo de la Interfaz ---

        // Función ASÍNCRONA para obtener las categorías de la base de datos a través de la API
        private async Task CargarCategoriasAsync()
        {
            try
            {
                // Realizar la solicitud GET a la API
                HttpResponseMessage respuesta = await _httpClient.GetAsync(API_CATEGORIAS_URL);

                if (respuesta.IsSuccessStatusCode)
                {
                    // Leer el contenido JSON de la respuesta
                    string jsonCategorias = await respuesta.Content.ReadAsStringAsync();

                    // Deserializar el JSON a una lista de objetos Categoria
                    var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    // Nota: Asegúrate de que la clase Categoria en C# coincida con el DTO/Modelo de Categoria de tu API.
                    List<Categoria> categorias = JsonSerializer.Deserialize<List<Categoria>>(jsonCategorias, opciones);

                    // Asignar la lista al ComboBox
                    cmbCategoria.ItemsSource = categorias;
                    cmbCategoria.DisplayMemberPath = "Nombre"; // Mostrar el nombre de la categoría
                    cmbCategoria.SelectedValuePath = "Id";     // Usar el ID como valor subyacente

                    if (categorias.Count > 0)
                    {
                        cmbCategoria.SelectedIndex = 0; // Seleccionar el primer elemento por defecto
                    }
                }
                else
                {
                    // Si la API no responde con éxito
                    MessageBox.Show($"Error al obtener categorías: {respuesta.StatusCode}", "Error de API", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (HttpRequestException ex)
            {
                // Error de red (el servidor no está disponible)
                MessageBox.Show($"🚨 Error de conexión al cargar categorías: No se pudo conectar a la API en {API_CATEGORIAS_URL}.\nDetalle: {ex.Message}", "Error de Red", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                // Otros errores (deserialización, etc.)
                MessageBox.Show($"🔴 Ocurrió un error inesperado al cargar categorías: {ex.Message}", "Error General", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCargarImagen_Click(object sender, RoutedEventArgs e)
        {
            // Abre un diálogo para seleccionar un archivo de imagen
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Archivos de imagen (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    // 1. Mostrar la imagen en la vista previa
                    Uri fileUri = new Uri(openFileDialog.FileName);
                    imgProducto.Source = new BitmapImage(fileUri);

                    // 2. Leer la imagen y convertirla a Base64 para enviarla a la API
                    byte[] imageBytes = File.ReadAllBytes(openFileDialog.FileName);
                    _imagenBase64 = Convert.ToBase64String(imageBytes);

                    // Nota: Asegúrate de que tu API pueda manejar el tamaño de la cadena Base64.
                    MessageBox.Show("Imagen cargada y lista para el registro.", "Información");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al cargar la imagen: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            // Lógica para navegar a otra página o limpiar el formulario
            LimpiarFormulario();
        }

        private async void btnRegistrar_Click(object sender, RoutedEventArgs e)
        {
            // 1. Validar los datos del formulario antes de enviar
            if (!ValidarFormulario())
            {
                return; // Detiene la ejecución si hay errores de validación
            }

            // 2. Verificar si el producto ya existe por nombre
            bool existe = await VerificarExistenciaProductoAsync(txtNombreProducto.Text);

            if (existe)
            {
                MessageBox.Show($"El producto con el nombre '{txtNombreProducto.Text}' ya existe. Por favor, ingrese un nombre diferente.", "Advertencia de Duplicado", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 3. Crear el objeto Producto e inicializar campos automáticos
            Producto nuevoProducto = new Producto
            {
                Nombre = txtNombreProducto.Text,
                Descripcion = txtDescripcion.Text,

                // Asignación de campos automáticos solicitados:
                Disponible = true,
                FechaRegistro = DateTime.Now,
                FechaModificacion = DateTime.Now,

                // Usamos SelectedValue, que debe ser el ID de la categoría (int)
                Categoria = (int)cmbCategoria.SelectedValue,

                // Conversiones seguras para Precio y Stock
                Precio = decimal.Parse(txtPrecioUnitario.Text),
                Unidades = int.Parse(txtUnidades.Text)

                // Si la imagen va en el JSON, descomenta esto y añádela al modelo Producto:
                // ImagenBase64 = _imagenBase64
            };

            // 4. Llamar a la función de registro en la API
            await RegistrarProductoAsync(nuevoProducto);
        }

        // --- Funciones de Lógica de Negocio y Comunicación HTTP ---

        /// <summary>
        /// Verifica si un producto ya existe en la base de datos de la API por su nombre.
        /// </summary>
        /// <param name="nombre">El nombre del producto a verificar.</param>
        /// <returns>True si el producto ya existe, False en caso contrario.</returns>
        private async Task<bool> VerificarExistenciaProductoAsync(string nombre)
        {
            try
            {
                // URL completa para la verificación. Se codifica el nombre para la URL.
                string urlVerificacion = $"{API_PRODUCTO_POR_NOMBRE_URL}/{Uri.EscapeDataString(nombre)}";

                // Realizar la solicitud GET a la API
                HttpResponseMessage respuesta = await _httpClient.GetAsync(urlVerificacion);

                // Si la API devuelve 200 OK, significa que el producto fue encontrado (existe).
                if (respuesta.IsSuccessStatusCode)
                {
                    // Asumimos que un código de éxito (200) significa que el producto existe.
                    // Si tu API devuelve un 404 cuando no existe y 200 cuando existe, esta lógica es correcta.
                    return true;
                }

                // Si la API devuelve 404 Not Found (que es común si no existe el recurso) o cualquier otro código
                // que no sea éxito, asumimos que no existe.
                return false;
            }
            catch (HttpRequestException ex)
            {
                // Error de red o servidor no disponible.
                MessageBox.Show($"🚨 Error de conexión al verificar producto: {ex.Message}", "Error de Red", MessageBoxButton.OK, MessageBoxImage.Error);
                // Para evitar registrar por error si la API no está disponible, devolvemos 'true' o manejamos el error.
                // Aquí, devolvemos false para permitir continuar si no podemos confirmar la existencia (o podrías lanzar el error si prefieres detener la operación).
                return false;
            }
        }


        private bool ValidarFormulario()
        {
            // Validación básica de campos no vacíos
            if (string.IsNullOrWhiteSpace(txtNombreProducto.Text) ||
                string.IsNullOrWhiteSpace(txtDescripcion.Text) ||
                string.IsNullOrWhiteSpace(txtPrecioUnitario.Text) ||
                string.IsNullOrWhiteSpace(txtUnidades.Text) ||
                cmbCategoria.SelectedValue == null)
            {
                MessageBox.Show("Todos los campos obligatorios deben ser llenados.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Validación de tipos de datos (Precio y Stock)
            if (!decimal.TryParse(txtPrecioUnitario.Text, out _) || !int.TryParse(txtUnidades.Text, out _))
            {
                MessageBox.Show("El precio y las unidades deben ser números válidos.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void LimpiarFormulario()
        {
            txtNombreProducto.Clear();
            txtDescripcion.Clear();
            txtPrecioUnitario.Clear();
            txtUnidades.Clear();
            cmbCategoria.SelectedIndex = 0;
            imgProducto.Source = null;
            _imagenBase64 = string.Empty;
        }

        public async Task RegistrarProductoAsync(Producto nuevoProducto)
        {
            try
            {
                // 1. Serializar el objeto Producto a JSON
                // Usamos opciones para mejorar la legibilidad del JSON y manejar camelCase si es necesario
                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                string jsonProducto = JsonSerializer.Serialize(nuevoProducto, options);

                // 2. Crear el contenido HTTP
                var contenido = new StringContent(jsonProducto, Encoding.UTF8, "application/json");

                // 3. Enviar la solicitud POST
                // El endpoint es la URL base definida arriba
                HttpResponseMessage respuesta = await _httpClient.PostAsync(API_PRODUCTOS_URL, contenido);

                // 4. Procesar la respuesta
                if (respuesta.IsSuccessStatusCode)
                {
                    // Éxito (ej. 201 Created)
                    // La API devuelve un mensaje como "Producto creado con el id:X"
                    string exitoContenido = await respuesta.Content.ReadAsStringAsync();
                    MessageBox.Show($"Producto registrado con éxito.\nDetalle: {exitoContenido}", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    LimpiarFormulario();
                }
                else
                {
                    // Error de la API (ej. 400 Bad Request, 500 Internal Server Error)
                    string errorContenido = await respuesta.Content.ReadAsStringAsync();
                    MessageBox.Show($"Error al registrar: {respuesta.StatusCode}\nDetalle: {errorContenido}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (HttpRequestException ex)
            {
                // Error de red (No se pudo contactar el servidor)
                MessageBox.Show($"Error de conexión: No se pudo conectar a la API en {API_PRODUCTOS_URL}.\nDetalle: {ex.Message}", "Error de Red", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                // Otros errores (serialización, etc.)
                MessageBox.Show($"Ocurrió un error inesperado: {ex.Message}", "Error General", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}