using sweet_temptation_clienteEscritorio.dto;
using sweet_temptation_clienteEscritorio.servicios;
using System.Windows;

namespace sweet_temptation_clienteEscritorio.vista.admin
{
    public partial class wFormularioUsuario : Window
    {
        private readonly UsuarioService _usuarioService;
        private readonly UserResponseDTO? _usuarioEditar;
        private readonly bool _esEdicion;

        public wFormularioUsuario(UserResponseDTO? usuario = null)
        {
            InitializeComponent();
            _usuarioService = new UsuarioService();
            _usuarioEditar = usuario;
            _esEdicion = usuario != null;

            CargarRoles();
            ConfigurarFormulario();
        }

        private void CargarRoles()
        {
            cmbRol.Items.Clear();
            cmbRol.Items.Add(new RolItem { Id = 2, Nombre = "Empleado" });
            cmbRol.Items.Add(new RolItem { Id = 3, Nombre = "Cliente" });
            cmbRol.DisplayMemberPath = "Nombre";
            cmbRol.SelectedValuePath = "Id";
            cmbRol.SelectedIndex = 0;
        }

        private void ConfigurarFormulario()
        {
            if (_esEdicion && _usuarioEditar != null)
            {
                txtTitulo.Text = "Editar Usuario";
                txtUsuario.Text = _usuarioEditar.Usuario;
                txtNombre.Text = _usuarioEditar.Nombre;
                txtApellidos.Text = _usuarioEditar.Apellidos;
                txtCorreo.Text = _usuarioEditar.Correo;
                txtTelefono.Text = _usuarioEditar.Telefono;
                txtDireccion.Text = _usuarioEditar.Direccion;

                foreach (RolItem item in cmbRol.Items)
                {
                    if (item.Id == _usuarioEditar.IdRol)
                    {
                        cmbRol.SelectedItem = item;
                        break;
                    }
                }

                lblContrasena.Text = "Nueva Contraseña (opcional)";
            }
            else
            {
                txtTitulo.Text = "Crear Nueva Cuenta";
            }
        }

        private async void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarFormulario())
                return;

            btnGuardar.IsEnabled = false;
            btnGuardar.Content = "Guardando...";

            try
            {
                var rolSeleccionado = cmbRol.SelectedItem as RolItem;
                var request = new UserRequestDTO
                {
                    Usuario = txtUsuario.Text.Trim(),
                    Nombre = txtNombre.Text.Trim(),
                    Apellidos = txtApellidos.Text.Trim(),
                    Correo = txtCorreo.Text.Trim(),
                    Telefono = txtTelefono.Text.Trim(),
                    Direccion = txtDireccion.Text.Trim(),
                    IdRol = rolSeleccionado?.Id ?? 2
                };

                if (!string.IsNullOrEmpty(txtContrasena.Password))
                {
                    request.Contrasena = txtContrasena.Password;
                }

                if (_esEdicion && _usuarioEditar != null)
                {
                    var resultado = await _usuarioService.UpdateUsuarioAsync(_usuarioEditar.Id, request);

                    if (resultado.usuario != null)
                    {
                        MessageBox.Show("Usuario actualizado correctamente.",
                            "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                        DialogResult = true;
                        Close();
                    }
                    else
                    {
                        MessageBox.Show($"Error al actualizar: {resultado.mensaje}",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(txtContrasena.Password))
                    {
                        MessageBox.Show("La contraseña es requerida para crear un nuevo usuario.",
                            "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                        btnGuardar.IsEnabled = true;
                        btnGuardar.Content = "Guardar";
                        return;
                    }

                    var resultado = await _usuarioService.CreateUsuarioAsync(request);

                    if (resultado.usuario != null)
                    {
                        MessageBox.Show("Usuario creado correctamente.",
                            "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                        DialogResult = true;
                        Close();
                    }
                    else
                    {
                        MessageBox.Show($"Error al crear: {resultado.mensaje}",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                btnGuardar.IsEnabled = true;
                btnGuardar.Content = "Guardar";
            }
        }

        private bool ValidarFormulario()
        {
            if (string.IsNullOrWhiteSpace(txtUsuario.Text))
            {
                MessageBox.Show("El campo Usuario es requerido.",
                    "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtUsuario.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("El campo Nombre es requerido.",
                    "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtNombre.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtCorreo.Text))
            {
                MessageBox.Show("El campo Correo es requerido.",
                    "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtCorreo.Focus();
                return false;
            }

            if (!txtCorreo.Text.Contains("@"))
            {
                MessageBox.Show("Ingrese un correo válido.",
                    "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtCorreo.Focus();
                return false;
            }

            if (cmbRol.SelectedItem == null)
            {
                MessageBox.Show("Seleccione un rol.",
                    "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

    public class RolItem
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }
}

