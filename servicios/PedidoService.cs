using sweet_temptation_clienteEscritorio.dto;
using sweet_temptation_clienteEscritorio.resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace sweet_temptation_clienteEscritorio.servicios
{
    internal class PedidoService
    {
        private readonly HttpClient _httpClient;

        public PedidoService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(Constantes.URL);
            
            var token = App.Current.Properties["Token"]?.ToString();
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<(PedidoDTO pedidoActual, HttpStatusCode codigo, string mensaje)> ObtenerPedidoActualAsync(int idCliente , string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
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

        public async Task<(List<PedidoDTO> pedidos, HttpStatusCode codigo, string mensaje)> ObtenerPedidosAsync(int idEmpleado, string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var respuesta = await _httpClient.GetAsync($"pedido/pedidos?idCliente={idEmpleado}");
            if (respuesta.IsSuccessStatusCode)
            {
                var pedidos = await respuesta.Content.ReadFromJsonAsync<List<PedidoDTO>>();
                return (pedidos, respuesta.StatusCode, null);
            }
            else
            {
                string mensaje = await respuesta.Content.ReadAsStringAsync();
                return (null, respuesta.StatusCode, mensaje);
            }
        }

        public async Task<(PedidoDTO pedidoActualizado, HttpStatusCode codigo, string mensaje)> CambiarTotalPedidoAsync(int idPedido, Decimal total,string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
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

        public async Task<bool> CrearPedidoClienteAsync(int idCliente, string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var respuesta = await _httpClient.PostAsync($"pedido/nuevo?idCliente={idCliente}", null);
            if (respuesta.IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }

        public async Task<(PedidoDTO pedido, HttpStatusCode codigo, string mensaje)> CrearPedidoEmpleadoAsync(int idEmpleado, string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var respuesta = await _httpClient.PostAsync($"pedido/?idEmpleado={idEmpleado}", null);
            if (respuesta.IsSuccessStatusCode)
            {
                var pedido = await respuesta.Content.ReadFromJsonAsync<PedidoDTO>();
                return (pedido, respuesta.StatusCode, null);
            }
            else
            {
                string mensaje = await respuesta.Content.ReadAsStringAsync();
                return (null, respuesta.StatusCode, mensaje);
            }
        }

        public async Task<(HttpStatusCode codigo, string mensaje)> EliminarPedidoAsync(int idPedido, string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
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

        public async Task<(HttpStatusCode codigo, string mensaje)> CancelarPedidoAsync(int idPedido, int idCliente, string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
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

        public async Task<(List<PedidoDTO> pedidos, HttpStatusCode codigo, string mensaje)> ObtenerHistorialAsync(int idCliente, string token) {
            try {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Llamada al nuevo endpoint especializado
                var respuesta = await _httpClient.GetAsync($"pedido/historial?idCliente={idCliente}");

                if(respuesta.IsSuccessStatusCode) {
                    // Si el servidor devuelve 204 No Content, el ReadFromJsonAsync podría fallar, 
                    // así que validamos si hay contenido.
                    if(respuesta.StatusCode == HttpStatusCode.NoContent) {
                        return (new List<PedidoDTO>(), respuesta.StatusCode, null);
                    }

                    var pedidos = await respuesta.Content.ReadFromJsonAsync<List<PedidoDTO>>();
                    return (pedidos, respuesta.StatusCode, null);
                } else {
                    string mensaje = await respuesta.Content.ReadAsStringAsync();
                    return (null, respuesta.StatusCode, mensaje);
                }
            } catch(Exception ex) {
                return (null, HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
