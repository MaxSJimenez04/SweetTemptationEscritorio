using Newtonsoft.Json;
using sweet_temptation_clienteEscritorio.dto;
using sweet_temptation_clienteEscritorio.resources; 
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System;

namespace sweet_temptation_clienteEscritorio.servicios {

    public class PedidoPersonalizadoService {

        private readonly HttpClient _http;
        private const string BaseUrl = "api/pedidos-personalizados";

        public PedidoPersonalizadoService(HttpClient http) {
            _http = http;
            _http.BaseAddress = new Uri(Constantes.URL); 

            var token = App.Current.Properties["Token"]?.ToString();
            if (!string.IsNullOrEmpty(token)) {
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<List<PedidoPersonalizadoDTO>> ObtenerSolicitudesAsync() {
            var res = await _http.GetAsync(BaseUrl); 
            if(!res.IsSuccessStatusCode) return null;

            var json = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<PedidoPersonalizadoDTO>>(json);
        }

        public async Task<bool> MarcarAtendidoAsync(int id) {
            var res = await _http.PutAsync($"{BaseUrl}/{id}/aceptar", null); 
            return res.IsSuccessStatusCode;
        }
    }
}