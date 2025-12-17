using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sweet_temptation_clienteEscritorio.dto {
    public class SolicitudPersonalizadaRequestDTO {
        public int IdCliente { get; set; } 

        public string tamano { get; set; }
        public string saborBizcocho { get; set; }
        public string relleno { get; set; }
        public string cobertura { get; set; }

        public string especificaciones { get; set; }
        public string imagenUrl { get; set; }

        public string telefonoContacto { get; set; }
    }
}