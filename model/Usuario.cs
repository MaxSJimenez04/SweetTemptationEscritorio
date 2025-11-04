using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sweet_temptation_clienteEscritorio.model
{
    internal class Usuario
    {
        public int Id { get; set; }
        public string User {  get; set; }
        public string Contrasena { get; set; }
        public string Nombre { get; set; }
        public string apellidos { get; set; }
        public string correo {  get; set; }
        public string direccion {  get; set; }
        public string Telefono { get; set; }
        public DateTime fechaRegistro { get; set; }
        public DateTime fechaModificacion { get; set; }
        public int IdRol {  get; set; }

    }
}
