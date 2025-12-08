using System;

namespace sweet_temptation_clienteEscritorio.dto
{
    public class UserResponseDTO
    {
        public int Id { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public int IdRol { get; set; }
        public DateTime? FechaRegistro { get; set; }

        public string NombreCompleto => $"{Nombre} {Apellidos}";
        
        public string RolNombre => IdRol switch
        {
            1 => "Administrador",
            2 => "Empleado",
            3 => "Cliente",
            _ => "Desconocido"
        };
    }
}







