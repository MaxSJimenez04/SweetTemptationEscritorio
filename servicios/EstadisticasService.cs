using sweet_temptation_clienteEscritorio.dto;
using sweet_temptation_clienteEscritorio.resources;
using System;
using System.Collections.Generic;
using System.Globalization;
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
    internal class EstadisticasService
    {
        private readonly HttpClient _httpClient;

        public EstadisticasService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(Constantes.URL);
        }

        public async Task<(List<EstadisticaProductoDTO> productos, HttpStatusCode codigo, string mensaje)> ObtenerEstadisticasProductosAsync(DateTime fechaInicio, 
            DateTime fechaFin, string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            string fechaInicioStr = fechaInicio.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            string fechaFinStr = fechaFin.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var respuesta = await _httpClient.GetAsync($"estadisticas/productos/?fechaInicio={fechaInicioStr}&fechaFin={fechaFinStr}");

            if (respuesta.IsSuccessStatusCode)
            {
                var productos = await respuesta.Content.ReadFromJsonAsync<List<EstadisticaProductoDTO>>();
                return (productos, respuesta.StatusCode, null);
            }
            else
            {
                string mensaje = await respuesta.Content.ReadAsStringAsync();
                return(null, respuesta.StatusCode, mensaje);
            }
        }

        public async Task<(List<EstadisticaVentaProductoDTO> estadisticas, HttpStatusCode codigo, string mensaje)> ObtenerEstadisticasProductoAsync(DateTime fechaInicio,
            DateTime fechaFin, int idProducto, string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            string fechaInicioStr = fechaInicio.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            string fechaFinStr = fechaFin.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var respuesta = await _httpClient.GetAsync($"estadisticas/productos/{idProducto}/?fechaInicio={fechaInicioStr}&fechaFin={fechaFinStr}");

            if (respuesta.IsSuccessStatusCode)
            {
                var productos = await respuesta.Content.ReadFromJsonAsync<List<EstadisticaVentaProductoDTO>>();
                return(productos, respuesta.StatusCode,null);
            }
            else
            {
                string mensaje = await respuesta.Content.ReadAsStringAsync();
                return(null, respuesta.StatusCode,mensaje);
            }
        }
    }
}
