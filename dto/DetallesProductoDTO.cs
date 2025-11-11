using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sweet_temptation_clienteEscritorio.dto
{
    internal class DetallesProductoDTO
    {
        public int id { get; set; }
        public int cantidad { get; set; }
        public string nombre { get; set; }
        public Decimal precio { get; set; }
        public Decimal subtotal { get; set; }
        public int idProducto { get; set; }
    }
}
