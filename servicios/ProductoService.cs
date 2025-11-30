using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
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

            _httpClient.BaseAddress = new Uri(Constantes.URL + "producto/");
        }

        // GET - obtener los productos
        public async Task<(List<ProductoDTO> productos, HttpStatusCode codigo, string mensaje)> ObtenerProductosAsync()
        {
            var respuesta = await _httpClient.GetAsync("");

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

        // GET - obtener producto por id
        public async Task<(ProductoDTO producto, HttpStatusCode codigo, string mensaje)> ObtenerProductoAsync(int idProducto)
        {
            // Ejemplo: GET /producto/5
            var respuesta = await _httpClient.GetAsync($"{idProducto}");

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

        // Get - obtener productos por categoria
        public async Task<(List<ProductoDTO> productos, HttpStatusCode codigo, string mensaje)> ObtenerPorCategoriaAsync(int idCategoria)
        {
            var respuesta = await _httpClient.GetAsync($"categoria/{idCategoria}");

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

        // POST - Crear un producto
        public async Task<(ProductoDTO productoCreado, HttpStatusCode codigo, string mensaje)> CrearProductoAsync(ProductoDTO nuevoProducto)
        {
            var respuesta = await _httpClient.PostAsJsonAsync("nuevo", nuevoProducto);

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

        // PUT - actualizar producto
        public async Task<(ProductoDTO productoActualizado, HttpStatusCode codigo, string mensaje)> ActualizarProductoAsync(int idProducto, ProductoDTO productoActualizado)
        {
            var respuesta = await _httpClient.PutAsJsonAsync($"{idProducto}", productoActualizado);

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

        // PUT - actualizar inventario
        public async Task<(ProductoDTO productoActualizado, HttpStatusCode codigo, string mensaje)> ModificarInventarioSync(int idProducto, int unidades)
        {
            var body = new { unidades = unidades };

            var respuesta = await _httpClient.PutAsJsonAsync($"{idProducto}/inventario", body);

            if (respuesta.IsSuccessStatusCode)
            {
                var producto = await respuesta.Content.ReadFromJsonAsync<ProductoDTO>();
                return (producto, respuesta.StatusCode, null);
            }
            else
            {
                var mensaje = await respuesta.Content.ReadAsStringAsync();
                return(null, respuesta.StatusCode, mensaje);
            }
        }

        // PUT - modificar disponibilidad del producto
        public async Task<(ProductoDTO productoActualizado, HttpStatusCode codigo, string mensaje)> CambiarDisponibilidadAsync(int idProducto, bool disponible)
        {
            var body = new { disponible = disponible };

            // Ejemplo: PUT /producto/5/disponible
            var respuesta = await _httpClient.PutAsJsonAsync($"{idProducto}/disponible", body);

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

        // DELETE - eliminar producto
        public async Task<(HttpStatusCode codigo, string mensaje)> EliminarProductoAsync(int idProducto)
        {
            var respuesta = await _httpClient.DeleteAsync($"{idProducto}");

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

