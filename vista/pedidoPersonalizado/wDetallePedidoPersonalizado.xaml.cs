using sweet_temptation_clienteEscritorio.dto;
using sweet_temptation_clienteEscritorio.servicios;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Net.Http;
using System;

namespace sweet_temptation_clienteEscritorio.vista.pedidoPersonalizado {
    /// <summary>
    /// Lógica de interacción para wDetallePedidoPersonalizado.xaml
    /// </summary>
    public partial class wDetallePedidoPersonalizado : Page {
        private readonly PedidoPersonalizadoDTO _pedido;

        private readonly PedidoPersonalizadoService _service =
            new PedidoPersonalizadoService(new HttpClient());

        public wDetallePedidoPersonalizado(PedidoPersonalizadoDTO pedido) {
            InitializeComponent();
            _pedido = pedido;
            DataContext = pedido; 
            CargarImagen(); 
        }

        private void CargarImagen() {
            string imageUrl = _pedido.imagenUrl;

            imgPastel.Visibility = Visibility.Collapsed;
            iconPlaceholder.Visibility = Visibility.Visible;

            if(!string.IsNullOrEmpty(imageUrl)) {
                try {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();

                    bitmap.UriSource = new Uri(imageUrl);

                    bitmap.CacheOption = BitmapCacheOption.OnLoad;

                    bitmap.EndInit();

                    imgPastel.Source = bitmap;
                    imgPastel.Visibility = Visibility.Visible;
                    iconPlaceholder.Visibility = Visibility.Collapsed;

                } catch(UriFormatException) {
                    MessageBox.Show("Advertencia: El formato de la URL de la imagen no es válido.", "Error de URL", MessageBoxButton.OK, MessageBoxImage.Warning);
                } catch(System.IO.FileNotFoundException) {
                    MessageBox.Show($"Advertencia: Archivo de imagen no encontrado en la ruta proporcionada: {imageUrl}", "Archivo No Encontrado", MessageBoxButton.OK, MessageBoxImage.Warning);
                } catch(Exception ex) {
                    MessageBox.Show($"Error desconocido al cargar la imagen: {ex.Message}", "Error de Carga", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            } else {
                imgPastel.Visibility = Visibility.Collapsed;
                iconPlaceholder.Visibility = Visibility.Visible;
            }
        }

        private async void BtnAtendido_Click(object sender, RoutedEventArgs e) {
            btnAtendido.IsEnabled = false;

            bool ok = await _service.MarcarAtendidoAsync(_pedido.id);

            if(ok) {
                MessageBox.Show("Pedido marcado como atendido (Aceptado).", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                if(NavigationService.CanGoBack) {
                    NavigationService.GoBack();
                }
            } else {
                MessageBox.Show("Error al marcar pedido como atendido. Verifique su conexión y autorización.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            btnAtendido.IsEnabled = true;
        }
    }
}