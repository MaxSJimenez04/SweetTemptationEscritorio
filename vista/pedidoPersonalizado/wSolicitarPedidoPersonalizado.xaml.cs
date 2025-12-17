using sweet_temptation_clienteEscritorio.servicios;
using sweet_temptation_clienteEscritorio.dto;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Net.Http;
using Microsoft.Win32;
using System.Windows.Media.Imaging; 
using System.IO; 

namespace sweet_temptation_clienteEscritorio.vista.pedidoPersonalizado {
    /// <summary>
    /// Lógica de interacción para wSolicitarPedidoPersonalziado.xaml
    /// </summary>
    public partial class wSolicitarPedidoPersonalziado : Page {

        private string _rutaImagenSeleccionada = "";

        private readonly PedidoCustomService _pedidoCustomService =
            new PedidoCustomService(new HttpClient());

        private int IdCliente {
            get {
                if(Application.Current.Properties.Contains("Id") &&
                   Application.Current.Properties["Id"] is int id) {
                    return id;
                }
                MessageBox.Show("Error: No se encontró el IdCliente en la sesión.", "Error de Sesión", MessageBoxButton.OK, MessageBoxImage.Error);
                return 0;
            }
        }
        private string TelefonoCliente {
            get {
                if(Application.Current.Properties.Contains("Telefono") &&
                   Application.Current.Properties["Telefono"] is string telefono &&
                   !string.IsNullOrEmpty(telefono)) {

                    return telefono;
                }

                MessageBox.Show("Error: No se encontró el teléfono del cliente en la sesión o está vacío. Asegúrate de que el API lo devuelva y lo guardes correctamente.", "Error de Sesión", MessageBoxButton.OK, MessageBoxImage.Error);
                return string.Empty;
            }
        }

        public wSolicitarPedidoPersonalziado() {
            InitializeComponent();
            if(IdCliente == 0) {
                MessageBox.Show("Debe iniciar sesión para realizar un pedido personalizado.");
            }
        }

        private void btnClickSubirImagen(object sender, RoutedEventArgs e) {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "Archivos de imagen|*.jpg;*.jpeg;*.png";

            if(openFileDialog.ShowDialog() == true) {
                try {
                    string rutaArchivo = openFileDialog.FileName;

                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.UriSource = new Uri(rutaArchivo);
                    bitmap.EndInit();

                    imgPastel.Source = bitmap;
                    imgPastel.Visibility = Visibility.Visible;
                    iconPlaceholder.Visibility = Visibility.Collapsed;

                    _rutaImagenSeleccionada = rutaArchivo;

                } catch(Exception ex) {
                    MessageBox.Show("Error al cargar la imagen: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    _rutaImagenSeleccionada = "";
                }
            }
        }

        private async void btnEnviarSolicitud_Click(object sender, RoutedEventArgs e) {

            int clienteId = IdCliente;
            string telefono = TelefonoCliente;

            if(clienteId == 0 || string.IsNullOrEmpty(telefono)) {
                return;
            }

            if(!ValidarCampos()) {
                MessageBox.Show("Complete todos los campos obligatorios", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var solicitud = new SolicitudPersonalizadaRequestDTO {
                IdCliente = clienteId,
                tamano = (cbTamaño.SelectedItem as ComboBoxItem)?.Content.ToString(),
                saborBizcocho = (cbSaborBizcocho.SelectedItem as ComboBoxItem)?.Content.ToString(),
                relleno = (cbRelleno.SelectedItem as ComboBoxItem)?.Content.ToString(),
                cobertura = (cbCobertura.SelectedItem as ComboBoxItem)?.Content.ToString(),
                especificaciones = txtEspecificaciones.Text,
                telefonoContacto = telefono,
                imagenUrl = _rutaImagenSeleccionada
            };

            try {
                btnEnviarSolicitud.IsEnabled = false;

                bool exito = await _pedidoCustomService.EnviarSolicitudAsync(solicitud);

                MessageBox.Show(exito
                    ? "Solicitud enviada correctamente"
                    : "Error al enviar solicitud. Verifique la conexión o autenticación.",
                    "Resultado de Solicitud", MessageBoxButton.OK,
                    exito ? MessageBoxImage.Information : MessageBoxImage.Error);

                if(exito) {
                    LimpiarCampos();
                }

            } catch(Exception ex) {
                MessageBox.Show("Error de conexión: " + ex.Message, "Error de Red", MessageBoxButton.OK, MessageBoxImage.Error);
            } finally {
                btnEnviarSolicitud.IsEnabled = true;
            }
        }


        private bool ValidarCampos() {
            return cbSaborBizcocho.SelectedItem != null &&
                   cbCobertura.SelectedItem != null &&
                   cbRelleno.SelectedItem != null &&
                   cbTamaño.SelectedItem != null;
        }

        private void LimpiarCampos() {
            cbSaborBizcocho.SelectedIndex = -1;
            cbCobertura.SelectedIndex = -1;
            cbRelleno.SelectedIndex = -1;
            cbTamaño.SelectedIndex = -1;
            txtEspecificaciones.Text = string.Empty;

            imgPastel.Source = null;
            imgPastel.Visibility = Visibility.Collapsed;
            iconPlaceholder.Visibility = Visibility.Visible;
            _rutaImagenSeleccionada = ""; 
        }
    }
}