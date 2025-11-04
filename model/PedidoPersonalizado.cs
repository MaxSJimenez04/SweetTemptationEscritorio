using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sweet_temptation_clienteEscritorio.model
{
    internal class PedidoPersonalizado
    {
        public int id {  get; set; }
        public int idPedido { get; set; }
        public string tamano { get; set; }
        public string saborBizcocho { get; set; }
        public string relleno { get; set; }
        public string cobertura { get; set; }
        public string especificaciones { get; set; }
        public DateTime fechaCompra {  get; set; }
        public DateTime fechaSolicitud { get; set; }

    }
}
