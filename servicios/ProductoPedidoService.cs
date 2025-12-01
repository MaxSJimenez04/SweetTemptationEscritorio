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

namespace sweet_temptation_clienteEscritorio.servicios
{
    internal class ProductoPedidoService
    {
        private readonly HttpClient _httpClient;

        public ProductoPedidoService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(Constantes.URL + "pedido/");
        }

        public async Task<(List<DetallesProductoDTO> productos, HttpStatusCode codigo, string mensaje)> obtenerProductosAsync(int idPedido,string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var respuesta = await _httpClient.GetAsync($"{idPedido}/?idPedido={idPedido}");

            if (respuesta.IsSuccessStatusCode)
            {
                var productos = await respuesta.Content.ReadFromJsonAsync<List<DetallesProductoDTO>>();
                return (productos, respuesta.StatusCode, null);
            }
            else
            {
                var mensaje = await respuesta.Content.ReadAsStringAsync();
                return (null, respuesta.StatusCode, mensaje);
            }
        }

        public async Task<(ProductoPedidoDTO productoNuevo, HttpStatusCode codigo, string mensaje)> crearProductoAsync(int idProducto, int idPedido, int cantidad, string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var respuesta = await _httpClient.PostAsync($"{idPedido}/?idProducto={idProducto}&idPedido={idPedido}&cantidad={cantidad}", null);

            if (respuesta.IsSuccessStatusCode)
            {
                var nuevoPedidoProducto = await respuesta.Content.ReadFromJsonAsync<ProductoPedidoDTO>();
                return(nuevoPedidoProducto, respuesta.StatusCode, null);
            }
            else 
            {
                var mensaje = await respuesta.Content.ReadAsStringAsync();
                return (null, respuesta.StatusCode, mensaje);
            }
        }

        public async Task<(ProductoPedidoDTO productoActualizado, HttpStatusCode codigo, string mensaje)> actualizarProductoAsync( int idPedido,
            ProductoPedidoDTO productoActualizado, string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var respuesta = await _httpClient.PutAsJsonAsync<ProductoPedidoDTO>($"{idPedido}/", productoActualizado);
            if (respuesta.IsSuccessStatusCode)
            {
                var producto = await respuesta.Content.ReadFromJsonAsync<ProductoPedidoDTO>();
                return (producto, respuesta.StatusCode, null);
            }
            else
            {
                var mensaje = await respuesta.Content.ReadAsStringAsync();
                return (null, respuesta.StatusCode, mensaje);
            }
        }

        public async Task<(HttpStatusCode codigo, string mensaje)> eliminarProductoAsync(int idPedido, int idProducto, string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var respuesta = await _httpClient.DeleteAsync($"{idPedido}/?idProducto={idProducto}");
            if (respuesta.IsSuccessStatusCode)
            {
                return (respuesta.StatusCode, null);
            }
            else
            {
                var mensaje = await respuesta.Content.ReadAsStringAsync();
                return (respuesta.StatusCode, mensaje);
            }
        }

        public async Task<(HttpStatusCode codigo, string mensaje)> comprarProductosAsync(int idPedido, List<DetallesProductoDTO> productoDTOs, string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var respuesta = await _httpClient.PutAsJsonAsync<List<DetallesProductoDTO>>($"{idPedido}/?", productoDTOs);
            if (respuesta.IsSuccessStatusCode)
            {
                return (respuesta.StatusCode, null);
            }
            else
            {
                var mensaje = await respuesta.Content.ReadAsStringAsync();
                return (respuesta.StatusCode, mensaje);
            }
        }


    }
}
