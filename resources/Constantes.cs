using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sweet_temptation_clienteEscritorio.resources
{
    internal class Constantes
    {
        public const string PUERTO = "8080";
        public const string PUERTO_GRPC = "9090";
        public const string IP = "localhost";
        public const string URL = "http://" + IP + ":" + PUERTO + "/";
        public const string URL_GRPC = "http://" + IP + ":" + PUERTO_GRPC;
        public const int IVA = 16;
    }
}
