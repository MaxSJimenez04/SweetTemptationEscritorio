using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sweet_temptation_clienteEscritorio.dto
{
    internal class DetallesArchivoDTO
    {
        public int id { get; set; }
        public DateTime fechaRegistro { get; set; }
        public string extension { get; set; }
        public string ruta { get; set; }
    }
}
