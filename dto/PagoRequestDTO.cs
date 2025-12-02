using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sweet_temptation_clienteEscritorio.dto
{
    class PagoRequestDTO
    {
        public string TipoPago { get; set; }
        public decimal MontoPagado { get; set; }
        public string DetallesCuenta { get; set; }
    }
}
