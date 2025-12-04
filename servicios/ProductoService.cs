using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using sweet_temptation_clienteEscritorio.dto;
using sweet_temptation_clienteEscritorio.resources;

namespace sweet_temptation_clienteEscritorio.servicios
{
    internal class ProductoService
    {
        private readonly HttpClient _httpClient;

        public ProductoService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            string url = Constantes.URL.EndsWith("/") ? Constantes.URL : Constantes.URL + "/";
            _httpClient.BaseAddress = new Uri(url);
        }

        private void ConfigurarToken(string token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        // GET - Obtener todos los productos
        // RUTA API: /producto/todos
        public async Task<(List<ProductoDTO> productos, HttpStatusCode codigo, string mensaje)> ObtenerProductosAsync(string token)
        {
            ConfigurarToken(token);

            try
            {
                var respuesta = await _httpClient.GetAsync("producto/todos");

                if (respuesta.IsSuccessStatusCode)
                {
                    var productos = await respuesta.Content.ReadFromJsonAsync<List<ProductoDTO>>();
                    return (productos, respuesta.StatusCode, null);
                }
                else
                {
                    var mensaje = await respuesta.Content.ReadAsStringAsync();
                    return (null, respuesta.StatusCode, mensaje);
                }
            }
            catch (Exception ex)
            {
                return (null, HttpStatusCode.ServiceUnavailable, "Error de conexión: " + ex.Message);
            }
        }

        // GET - Obtener producto por ID
        // RUTA API: /producto/{id}
        public async Task<(ProductoDTO producto, HttpStatusCode codigo, string mensaje)> ObtenerProductoAsync(int idProducto, string token)
        {
            ConfigurarToken(token);

            var respuesta = await _httpClient.GetAsync($"producto/{idProducto}");

            if (respuesta.IsSuccessStatusCode)
            {
                var producto = await respuesta.Content.ReadFromJsonAsync<ProductoDTO>();
                return (producto, respuesta.StatusCode, null);
            }
            else
            {
                var mensaje = await respuesta.Content.ReadAsStringAsync();
                return (null, respuesta.StatusCode, mensaje);
            }
        }

        // POST - Crear un producto
        // RUTA API: /producto/nuevo
        public async Task<(int idProducto, HttpStatusCode codigo, string mensaje)> CrearProductoAsync(ProductoDTO producto, string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var respuesta = await _httpClient.PostAsJsonAsync("producto/nuevo", producto);

            if (!respuesta.IsSuccessStatusCode)
            {
                string mensaje = await respuesta.Content.ReadAsStringAsync();
                return (0, respuesta.StatusCode, mensaje);
            }

            int id = await respuesta.Content.ReadFromJsonAsync<int>();

            return (id, respuesta.StatusCode, "OK");
        }


        // PUT - Actualizar producto
        // RUTA API: /producto/{id}
        public async Task<(ProductoDTO productoActualizado, HttpStatusCode codigo, string mensaje)> ActualizarProductoAsync(int idProducto, ProductoDTO productoActualizado, string token)
        {
            ConfigurarToken(token);

            var respuesta = await _httpClient.PutAsJsonAsync($"producto/{idProducto}", productoActualizado);

            if (respuesta.IsSuccessStatusCode)
            {
                var producto = await respuesta.Content.ReadFromJsonAsync<ProductoDTO>();
                return (producto, respuesta.StatusCode, null);
            }
            else
            {
                var mensaje = await respuesta.Content.ReadAsStringAsync();
                return (null, respuesta.StatusCode, mensaje);
            }
        }

        // DELETE - Eliminar producto
        // RUTA API: /producto/{id}
        public async Task<(HttpStatusCode codigo, string mensaje)> EliminarProductoAsync(int idProducto, string token)
        {
            ConfigurarToken(token);

            var respuesta = await _httpClient.DeleteAsync($"producto/{idProducto}");

            if (respuesta.IsSuccessStatusCode)
            {
                var mensaje = await respuesta.Content.ReadAsStringAsync();
                return (respuesta.StatusCode, mensaje);
            }
            else
            {
                var mensaje = await respuesta.Content.ReadAsStringAsync();
                return (respuesta.StatusCode, mensaje);
            }
        }

        // TODO - por terminar
        // GET - Por Categoria 
        public async Task<(List<ProductoDTO> productos, HttpStatusCode codigo, string mensaje)> ObtenerPorCategoriaAsync(int idCategoria, string token)
        {
            ConfigurarToken(token);
            
            var respuesta = await _httpClient.GetAsync($"producto/categoria/{idCategoria}");

            if (respuesta.IsSuccessStatusCode)
            {
                var lista = await respuesta.Content.ReadFromJsonAsync<List<ProductoDTO>>();
                return (lista, respuesta.StatusCode, null);
            }
            else
            {
                var mensaje = await respuesta.Content.ReadAsStringAsync();
                return (null, respuesta.StatusCode, mensaje);
            }
        }

        // GET - Obtener todas las categorías para el ComboBox
        // RUTA API: /categoria/todos
        public async Task<(List<CategoriaDTO> categorias, HttpStatusCode codigo, string mensaje)> ObtenerCategoriasAsync(string token)
        {
            ConfigurarToken(token);

            try
            {
                var respuesta = await _httpClient.GetAsync("categoria/todos");

                if (respuesta.IsSuccessStatusCode)
                {
                    var lista = await respuesta.Content.ReadFromJsonAsync<List<CategoriaDTO>>();
                    return (lista, respuesta.StatusCode, null);
                }
                else
                {
                    var mensaje = await respuesta.Content.ReadAsStringAsync();
                    return (null, respuesta.StatusCode, mensaje);
                }
            }
            catch (Exception ex)
            {
                return (null, HttpStatusCode.ServiceUnavailable, ex.Message);
            }
        }
    }


}