using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sweet_temptation_clienteEscritorio.dto
{
    internal class ProductoDTO
    {
        public int IdProducto { get; set; }
        public string Nombre { get; set; }
        public string Descripcion {  get; set; }
        public Decimal Precio {  get; set; }
        public Boolean Disponible {  get; set; }
        public int Unidades {  get; set; }
        public DateTime fechaRegistro {  get; set; }
        public DateTime fechaModificacion { get; set; }
        public int categoria {  get; set; }
    }
}
