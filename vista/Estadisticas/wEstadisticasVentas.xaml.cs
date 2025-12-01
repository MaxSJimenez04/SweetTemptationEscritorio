using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Net.Http; // Necesario para la comunicación con la API

namespace sweet_temptation_clienteEscritorio.vista.Estadisticas
{
    /// <summary>
    /// Lógica de interacción para wEstadisticasVentas.xaml
    /// </summary>
    public partial class wEstadisticasVentas : Page
    {
    
        private const string API_ESTADISTICAS_URL = "http://localhost:8080/reportes/ventas";

        private HttpClient _httpClient;

        public wEstadisticasVentas()
        {
            InitializeComponent();
            _httpClient = new HttpClient();

            // Iniciar la carga de datos del reporte al abrir la vista (opcional)
            // _ = CargarEstadisticasAsync(); 
        }

        /// <summary>
        /// Método para cargar los datos de estadísticas de la API.
        /// </summary>
        private async Task CargarEstadisticasAsync()
        {
            try
            {
                

                System.Windows.MessageBox.Show("Funcionalidad pendiente: Carga de datos de estadísticas.", "Información");
            }
            catch (HttpRequestException ex)
            {
                System.Windows.MessageBox.Show($"🚨 Error de conexión al cargar estadísticas: {ex.Message}", "Error de Red");
            }
        }

        /// <summary>
        /// Maneja el evento de clic para descargar el reporte de ventas o las estadísticas.
        /// </summary>
        private void btnDescargar_Click(object sender, RoutedEventArgs e)
        {
            // TODO

            System.Windows.MessageBox.Show("Funcionalidad pendiente: Se iniciaría la descarga del reporte.", "Descarga de Reporte");
        }
    }
}
