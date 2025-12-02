using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sweet_temptation_clienteEscritorio.dto
{
    class PagoResponseDTO
    {
        public int IdPago { get; set; }
        public string MensajeConfirmacion { get; set; }
        public decimal CambioDevuelto { get; set; }
        public decimal TotalPagado { get; set; }
    }
}
