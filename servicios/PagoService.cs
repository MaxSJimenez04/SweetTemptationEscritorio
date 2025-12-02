using sweet_temptation_clienteEscritorio.dto;
using sweet_temptation_clienteEscritorio.resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Json; // <-- ESTE ES EL ÚNICO QUE FALTABA

namespace sweet_temptation_clienteEscritorio.servicios {
    internal class PagoService {
        private readonly HttpClient _httpClient;

        public PagoService(HttpClient httpClient) {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(Constantes.URL);

            var token = App.Current.Properties["Token"]?.ToString();
            if(!string.IsNullOrEmpty(token)) {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        // POST /pago/{idPedido}
        public async Task<(PagoResponseDTO pago, HttpStatusCode codigo, string mensaje)>
            RegistrarPagoAsync(int idPedido, PagoRequestDTO request, string token) {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var respuesta = await _httpClient.PostAsJsonAsync($"pago/{idPedido}", request);

            if(respuesta.IsSuccessStatusCode) {
                var pago = await respuesta.Content.ReadFromJsonAsync<PagoResponseDTO>();
                return (pago, respuesta.StatusCode, null);
            } else {
                string mensaje = await respuesta.Content.ReadAsStringAsync();
                return (null, respuesta.StatusCode, mensaje);
            }
        }

        // GET /pago/pedido/{idPedido}
        public async Task<(PagoDTO pago, HttpStatusCode codigo, string mensaje)>
            ConsultarPagoAsync(int idPedido, string token) {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var respuesta = await _httpClient.GetAsync($"pago/pedido/{idPedido}");

            if(respuesta.IsSuccessStatusCode) {
                var pago = await respuesta.Content.ReadFromJsonAsync<PagoDTO>();
                return (pago, respuesta.StatusCode, null);
            } else {
                string mensaje = await respuesta.Content.ReadAsStringAsync();
                return (null, respuesta.StatusCode, mensaje);
            }
        }
    }
}
