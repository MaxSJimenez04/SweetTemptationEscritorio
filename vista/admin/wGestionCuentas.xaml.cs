using sweet_temptation_clienteEscritorio.dto;
using sweet_temptation_clienteEscritorio.servicios;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace sweet_temptation_clienteEscritorio.vista.admin
{
    public partial class wGestionCuentas : Page
    {
        private readonly UsuarioService _usuarioService;
        private ObservableCollection<UserResponseDTO> _usuarios;

        public wGestionCuentas()
        {
            InitializeComponent();
            _usuarioService = new UsuarioService();
            _usuarios = new ObservableCollection<UserResponseDTO>();

            Loaded += async (s, e) => await CargarUsuariosAsync();
        }

        private async Task CargarUsuariosAsync()
        {
            loadingOverlay.Visibility = Visibility.Visible;

            var resultado = await _usuarioService.GetUsuariosAsync();

            if (resultado.usuarios != null)
            {
                _usuarios.Clear();
                foreach (var usuario in resultado.usuarios)
                {
                    _usuarios.Add(usuario);
                }
                dgUsuarios.ItemsSource = _usuarios;
            }
            else
            {
                MessageBox.Show($"Error al cargar usuarios: {resultado.mensaje}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            loadingOverlay.Visibility = Visibility.Collapsed;
        }

        private async void BtnCrearCuenta_Click(object sender, RoutedEventArgs e)
        {
            var formulario = new wFormularioUsuario();
            formulario.Owner = Window.GetWindow(this);

            if (formulario.ShowDialog() == true)
            {
                await CargarUsuariosAsync();
            }
        }

        private async void BtnActualizar_Click(object sender, RoutedEventArgs e)
        {
            await CargarUsuariosAsync();
        }

        private async void BtnEditar_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var usuario = button?.Tag as UserResponseDTO;

            if (usuario != null)
            {
                var formulario = new wFormularioUsuario(usuario);
                formulario.Owner = Window.GetWindow(this);

                if (formulario.ShowDialog() == true)
                {
                    await CargarUsuariosAsync();
                }
            }
        }

        private async void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var usuario = button?.Tag as UserResponseDTO;

            if (usuario != null)
            {
                var resultado = MessageBox.Show(
                    $"¿Estás seguro de que deseas eliminar al usuario '{usuario.Usuario}'?\n\nEsta acción no se puede deshacer.",
                    "Confirmar eliminación",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (resultado == MessageBoxResult.Yes)
                {
                    loadingOverlay.Visibility = Visibility.Visible;

                    var response = await _usuarioService.DeleteUsuarioAsync(usuario.Id);

                    if (response.mensaje == null)
                    {
                        MessageBox.Show("Usuario eliminado correctamente.",
                            "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                        await CargarUsuariosAsync();
                    }
                    else
                    {
                        MessageBox.Show($"Error al eliminar usuario: {response.mensaje}",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    loadingOverlay.Visibility = Visibility.Collapsed;
                }
            }
        }
    }
}

