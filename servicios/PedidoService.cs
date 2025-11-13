using sweet_temptation_clienteEscritorio.dto;
using sweet_temptation_clienteEscritorio.resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace sweet_temptation_clienteEscritorio.servicios
{
    internal class PedidoService
    {
        private readonly HttpClient _httpClient;

        public PedidoService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(Constantes.URL);
        }

        public async Task<(PedidoDTO pedidoActual, HttpStatusCode codigo, string mensaje)> ObtenerPedidoActualAsync(int idCliente)
        {
            var respuesta = await _httpClient.GetAsync($"pedido/actual?idCliente={idCliente}");

            if (respuesta.IsSuccessStatusCode)
            {
                var pedido = await respuesta.Content.ReadFromJsonAsync<PedidoDTO>();
                return(pedido, respuesta.StatusCode, null);
            }
            else
            {
                string mensaje = await respuesta.Content.ReadAsStringAsync();
                return(null, respuesta.StatusCode, mensaje);
            }
        }

        public async Task<(PedidoDTO pedidoActualizado, HttpStatusCode codigo, string mensaje)> CambiarTotalPedidoAsync(int idPedido, Decimal total)
        {
            var respuesta = await _httpClient.PutAsJsonAsync<Decimal>($"pedido/{idPedido}/recalcular?idPedido={idPedido}", total);

            if (respuesta.IsSuccessStatusCode)
            {
                var pedido = await respuesta.Content.ReadFromJsonAsync<PedidoDTO>();
                return (pedido, respuesta.StatusCode, null);
            }
            else
            {
                string mensaje = await respuesta.Content.ReadAsStringAsync();
                return(null, respuesta.StatusCode, mensaje);
            }
        }

        public async Task<bool> CrearPedidoClienteAsync(int idCliente)
        {
            var respuesta = await _httpClient.PostAsync($"pedido/nuevo?idCliente={idCliente}", null);
            if (respuesta.IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }

        public async Task<(HttpStatusCode codigo, string mensaje)> EliminarPedidoAsync(int idPedido)
        {
            var respuesta = await _httpClient.DeleteAsync($"pedido/?idPedido={idPedido}");

            if (respuesta.IsSuccessStatusCode)
            {
                return (respuesta.StatusCode, null);
            }
            else
            {
                string mensaje = await respuesta.Content.ReadAsStringAsync();
                return (respuesta.StatusCode, mensaje);
            }
        }

        public async Task<(HttpStatusCode codigo, string mensaje)> CancelarPedidoAsync(int idPedido, int idCliente)
        {
            var respuesta = await _httpClient.PutAsync($"/pedido/?idPedido={idPedido}&idCliente={idCliente}", null);

            if (respuesta.IsSuccessStatusCode)
            {
                return (respuesta.StatusCode, null);
            }
            else
            {
                string mensaje = await respuesta.Content.ReadAsStringAsync();
                return (respuesta.StatusCode, mensaje);
            }


        }


        
    }
}
