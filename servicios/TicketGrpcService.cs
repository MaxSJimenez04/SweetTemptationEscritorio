using Grpc.Net.Client;
using sweet_temptation_clienteEscritorio.resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sweet_temptation_clienteEscritorio.servicios
{
    internal class TicketGrpcService
    {
        public async Task<String> DescargarTicketAsync(int idPedido)
        {
            using var canal = GrpcChannel.ForAddress(Constantes.URL_GRPC);

            var cliente = new TicketService.TicketServiceClient(canal);

            var peticionGrpc = new TicketRequest
            {
                IdPedido = idPedido
            };

            var respuesta = await cliente.generarTicketAsync(peticionGrpc);
            byte[] pdfBytes = respuesta.Pdf.ToByteArray();
            string rutaDescargas = Path.Combine( Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),"Downloads");
            string path = Path.Combine(rutaDescargas, respuesta.FileName);

            File.WriteAllBytes(path, pdfBytes);
            return (path);
        }
    }
}
