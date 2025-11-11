using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sweet_temptation_clienteEscritorio.dto
{
    public class ProductoPedidoDTO
    {
        public int Id { get; set; }
        public Decimal Subtotal { get; set; }
        public int Cantidad { get; set; }
        public int IdPedido { get; set; }
        public int IdProducto { get; set; }
    }
}
