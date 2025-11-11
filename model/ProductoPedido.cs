using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sweet_temptation_clienteEscritorio.model
{
    public class ProductoPedido
    {
        public int Id { get; set; }
        public Decimal Subtotal { get; set; }
        public int Cantidad { get; set; }
        public int IdPedido { get; set; }
        public int IdProducto { get; set; }

    }
}
