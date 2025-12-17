using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using sweet_temptation_clienteEscritorio.dto;
using sweet_temptation_clienteEscritorio.resources; 
using Newtonsoft.Json.Serialization; 
using System.Net.Http.Headers;

namespace sweet_temptation_clienteEscritorio.servicios {
    public class PedidoCustomService {

        private readonly HttpClient _http;
        private const string BaseUrl = "http://localhost:8080/api/pedidos-personalizados"; 

        public PedidoCustomService(HttpClient http) {
            _http = http;
            _http.BaseAddress = new Uri(Constantes.URL); 


            var token = App.Current.Properties["Token"]?.ToString();
            if(!string.IsNullOrEmpty(token)) {
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<bool> EnviarSolicitudAsync(SolicitudPersonalizadaRequestDTO solicitud) {

            var settings = new JsonSerializerSettings {

                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            var json = JsonConvert.SerializeObject(solicitud, settings);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.PostAsync(BaseUrl, content);
            return response.IsSuccessStatusCode;
        }
    }
}