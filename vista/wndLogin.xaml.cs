using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace sweet_temptation_clienteEscritorio.vista
{
    public partial class wndLogin : Window
    {
        private readonly HttpClient _httpClient;
        private const string API_BASE_URL = "http://localhost:8080";

        public wndLogin()
        {
            InitializeComponent();
            _httpClient = new HttpClient();
            
            // Manejar placeholder de contraseña manualmente
            txtContrasena.PasswordChanged += TxtContrasena_PasswordChanged;
        }

        private void TxtContrasena_PasswordChanged(object sender, RoutedEventArgs e)
        {
            placeholderContrasena.Visibility = string.IsNullOrEmpty(txtContrasena.Password) 
                ? Visibility.Visible 
                : Visibility.Collapsed;
        }

        private async void btnIniciarSesion_Click(object sender, RoutedEventArgs e)
        {
            string usuario = txtUsuario.Text.Trim();
            string contrasena = txtContrasena.Password;

            if (string.IsNullOrEmpty(usuario))
            {
                MessageBox.Show("Por favor ingresa tu usuario o correo.", "Campos requeridos", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtUsuario.Focus();
                return;
            }

            if (string.IsNullOrEmpty(contrasena))
            {
                MessageBox.Show("Por favor ingresa tu contraseña.", "Campos requeridos", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtContrasena.Focus();
                return;
            }

            btnIniciarSesion.IsEnabled = false;
            btnIniciarSesion.Content = "Iniciando...";

            try
            {
                var loginResponse = await LoginAsync(usuario, contrasena);
                
                if (loginResponse != null)
                {
                    App.Current.Properties["Token"] = loginResponse.Token;
                    App.Current.Properties["Id"] = loginResponse.Id;
                    App.Current.Properties["Nombre"] = loginResponse.Nombre;
                    App.Current.Properties["Correo"] = loginResponse.Correo;
                    App.Current.Properties["Rol"] = loginResponse.Rol;

                    Window menuWindow = loginResponse.Rol switch
                    {
                        "Administrador" => new wndMenuAdmin(),
                        "Empleado" => new wndMenuEmpleado(),
                        "Cliente" => new wndMenuCliente(),
                        _ => new wndMenuCliente()
                    };

                    menuWindow.Show();
                    this.Close();
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Error de conexión: No se pudo conectar con el servidor.\n\nAsegúrate de que la API esté ejecutándose.", 
                    "Error de conexión", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado: {ex.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                btnIniciarSesion.IsEnabled = true;
                btnIniciarSesion.Content = "Iniciar sesión";
            }
        }

        private async Task<LoginResponse?> LoginAsync(string usuario, string contrasena)
        {
            var loginRequest = new
            {
                usuario = usuario,
                contrasena = contrasena
            };

            var json = JsonSerializer.Serialize(loginRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{API_BASE_URL}/auth/login", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<LoginResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                MessageBox.Show("Usuario o contraseña incorrectos.", "Credenciales inválidas", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                MessageBox.Show($"Error del servidor: {response.StatusCode}\n{errorContent}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        private void linkOlvidasteContrasena_Click(object sender, MouseButtonEventArgs e)
        {
            var recuperarWindow = new wndRecuperarContrasena();
            recuperarWindow.Show();
            this.Close();
        }
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
    }
}
