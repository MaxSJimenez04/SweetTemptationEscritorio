using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sweet_temptation_clienteEscritorio.model
{
    internal class Pago
    {
        public int id {get; set;}
        public Decimal total {get; set;}
        public DateTime fechaPago {get; set;}
        public string tipoPago { get; set;}
        public string cuenta {  get; set;}
        public int idPedido { get; set;}

    }
}
