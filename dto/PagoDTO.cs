using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sweet_temptation_clienteEscritorio.dto
{
    internal class PagoDTO
    {
        public int Id { get; set; }
        public decimal Total { get; set; }
        public DateTime FechaPago { get; set; }
        public string TipoPago { get; set; }
        public string Cuenta { get; set; }
        public int IdPedido { get; set; }
    }
}
