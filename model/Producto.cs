using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sweet_temptation_clienteEscritorio.model
{
    internal class Producto
    {
        public int id {  get; set; }
        public string nombre { get; set; }
        public string descripcion {  get; set; }
        public Decimal precio { get; set; }
        public Boolean disponible { get; set; }
        public int unidades { get; set; }
        public DateTime fechaRegistro { get; set; }
        public DateTime fechaModificacion { get; set; }
        public int categoria { get; set; }

    }
}
