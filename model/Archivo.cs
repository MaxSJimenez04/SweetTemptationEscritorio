using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sweet_temptation_clienteEscritorio.model
{
    internal class Archivo
    {
        public int id {  get; set; }
        public DateTime fechaRegistro { get; set; }
        public string extension { get; set; }
        public byte[] datos { get; set; }
    }
}
