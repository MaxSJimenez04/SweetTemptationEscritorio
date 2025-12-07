using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace sweet_temptation_clienteEscritorio.vista
{
    public partial class wndRecuperarContrasena : Window
    {
        private readonly HttpClient _httpClient;
        private const string API_BASE_URL = "http://localhost:8080";

        public wndRecuperarContrasena()
        {
            InitializeComponent();
            _httpClient = new HttpClient();
        }

        private async void btnEnviarCodigo_Click(object sender, RoutedEventArgs e)
        {
            string correo = txtCorreo.Text.Trim();

            if (string.IsNullOrEmpty(correo))
            {
                MessageBox.Show("Por favor ingresa tu correo electrónico.", "Campo requerido", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtCorreo.Focus();
                return;
            }

            if (!IsValidEmail(correo))
            {
                MessageBox.Show("Por favor ingresa un correo electrónico válido.", "Correo inválido", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtCorreo.Focus();
                return;
            }

            btnEnviarCodigo.IsEnabled = false;
            btnEnviarCodigo.Content = "Enviando...";
            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                var result = await EnviarCodigoAsync(correo);
                
                if (result)
                {
                    MessageBox.Show("Se ha enviado un código a tu correo electrónico.", "Código enviado", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    var cambiarWindow = new wndCambiarContrasena();
                    cambiarWindow.Show();
                    this.Close();
                }
            }
            catch (HttpRequestException)
            {
                MessageBox.Show("Error de conexión: No se pudo conectar con el servidor.\n\nAsegúrate de que la API esté ejecutándose.", 
                    "Error de conexión", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado: {ex.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
                btnEnviarCodigo.IsEnabled = true;
                btnEnviarCodigo.Content = "Enviar Código";
            }
        }

        private async Task<bool> EnviarCodigoAsync(string correo)
        {
            var request = new { correo = correo };
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{API_BASE_URL}/auth/forgot-password", content);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                MessageBox.Show($"No se encontró una cuenta con ese correo electrónico.", "Correo no encontrado", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                MessageBox.Show($"Error del servidor: {response.StatusCode}\n{errorContent}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private void linkVolverLogin_Click(object sender, MouseButtonEventArgs e)
        {
            var loginWindow = new wndLogin();
            loginWindow.Show();
            this.Close();
        }
    }
}

