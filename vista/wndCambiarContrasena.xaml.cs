using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace sweet_temptation_clienteEscritorio.vista
{
    public partial class wndCambiarContrasena : Window
    {
        private readonly HttpClient _httpClient;
        private const string API_BASE_URL = "http://localhost:8080";

        public wndCambiarContrasena()
        {
            InitializeComponent();
            _httpClient = new HttpClient();
        }

        private void txtNuevaContrasena_PasswordChanged(object sender, RoutedEventArgs e)
        {
            placeholderNuevaContrasena.Visibility = string.IsNullOrEmpty(txtNuevaContrasena.Password) 
                ? Visibility.Visible 
                : Visibility.Collapsed;
        }

        private void txtConfirmarContrasena_PasswordChanged(object sender, RoutedEventArgs e)
        {
            placeholderConfirmarContrasena.Visibility = string.IsNullOrEmpty(txtConfirmarContrasena.Password) 
                ? Visibility.Visible 
                : Visibility.Collapsed;
        }

        private async void btnActualizar_Click(object sender, RoutedEventArgs e)
        {
            string token = txtToken.Text.Trim();
            string nuevaContrasena = txtNuevaContrasena.Password;
            string confirmarContrasena = txtConfirmarContrasena.Password;

            if (string.IsNullOrEmpty(token))
            {
                MessageBox.Show("Por favor ingresa el código que recibiste en tu correo.", "Campo requerido", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtToken.Focus();
                return;
            }

            if (token.Length != 8)
            {
                MessageBox.Show("El código debe tener exactamente 8 caracteres.", "Código inválido", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtToken.Focus();
                return;
            }

            if (string.IsNullOrEmpty(nuevaContrasena))
            {
                MessageBox.Show("Por favor ingresa tu nueva contraseña.", "Campo requerido", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtNuevaContrasena.Focus();
                return;
            }

            if (nuevaContrasena.Length < 6)
            {
                MessageBox.Show("La contraseña debe tener al menos 6 caracteres.", "Contraseña muy corta", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtNuevaContrasena.Focus();
                return;
            }

            if (string.IsNullOrEmpty(confirmarContrasena))
            {
                MessageBox.Show("Por favor confirma tu nueva contraseña.", "Campo requerido", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtConfirmarContrasena.Focus();
                return;
            }

            if (nuevaContrasena != confirmarContrasena)
            {
                MessageBox.Show("Las contraseñas no coinciden.", "Error de validación", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtConfirmarContrasena.Focus();
                return;
            }

            btnActualizar.IsEnabled = false;
            btnActualizar.Content = "Actualizando...";
            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                var result = await ActualizarContrasenaAsync(token, nuevaContrasena);
                
                if (result)
                {
                    MessageBox.Show("Tu contraseña ha sido actualizada correctamente.\n\nYa puedes iniciar sesión con tu nueva contraseña.", 
                        "Contraseña actualizada", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    var loginWindow = new wndLogin();
                    loginWindow.Show();
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
                btnActualizar.IsEnabled = true;
                btnActualizar.Content = "Actualizar Contraseña";
            }
        }

        private async Task<bool> ActualizarContrasenaAsync(string token, string nuevaContrasena)
        {
            var request = new 
            { 
                token = token, 
                nuevaContrasena = nuevaContrasena 
            };
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{API_BASE_URL}/auth/reset-password", content);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                
                if (errorContent.Contains("expirado"))
                {
                    MessageBox.Show("El código ha expirado. Por favor solicita uno nuevo.", "Código expirado", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else if (errorContent.Contains("inválido"))
                {
                    MessageBox.Show("El código ingresado no es válido. Verifica que lo hayas escrito correctamente.", "Código inválido", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    MessageBox.Show($"Error: {errorContent}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
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

        private void linkVolverLogin_Click(object sender, MouseButtonEventArgs e)
        {
            var loginWindow = new wndLogin();
            loginWindow.Show();
            this.Close();
        }
    }
}

