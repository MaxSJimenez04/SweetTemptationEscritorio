using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sweet_temptation_clienteEscritorio.dto
{
    internal class PedidoDTO
    {
        public int id {  get; set; }
        public DateTime fechaCompra {  get; set; }
        public Boolean actual {  get; set; }
        public Decimal total { get; set; }
        public int estado { get; set; }
        public Boolean personalizado { get; set; }
        public int idCliente { get; set; }
    }
}
