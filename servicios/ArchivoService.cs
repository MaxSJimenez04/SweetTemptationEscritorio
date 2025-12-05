using sweet_temptation_clienteEscritorio.dto;
using sweet_temptation_clienteEscritorio.resources;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace sweet_temptation_clienteEscritorio.servicios
{
    internal class ArchivoService
    {
        private readonly HttpClient _httpClient;

        public ArchivoService(HttpClient httpClient)
        {
            _httpClient = httpClient;

            // BaseAddress SI debe terminar con /
            // Ejemplo: http://localhost:8080/api/
            _httpClient.BaseAddress = new Uri(Constantes.URL);
        }


        // =====================================================================
        //    GUARDAR ARCHIVO
        // =====================================================================
        public async Task<(int idArchivo, HttpStatusCode codigo, string mensaje)> GuardarArchivoAsync(ArchivoDTO archivo, string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var json = System.Text.Json.JsonSerializer.Serialize(archivo);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // ✔ CORREGIDO (nada de "/" al inicio o final)
            var respuesta = await _httpClient.PostAsync("archivo", content);

            if (!respuesta.IsSuccessStatusCode)
            {
                string msg = await respuesta.Content.ReadAsStringAsync();
                return (0, respuesta.StatusCode, msg);
            }

            string texto = await respuesta.Content.ReadAsStringAsync();

            if (int.TryParse(texto, out int idArchivo))
                return (idArchivo, respuesta.StatusCode, null);

            return (0, respuesta.StatusCode, "Respuesta inesperada: " + texto);
        }


        // =====================================================================
        //    ASOCIAR ARCHIVO A PRODUCTO
        // =====================================================================
        public async Task<(HttpStatusCode codigo, string mensaje)> AsociarArchivoAsync(int idArchivo, int idProducto, string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            // ✔ CORREGIDO (sin slash inicial)
            var respuesta = await _httpClient.PostAsync(
                $"archivo/asociar/{idArchivo}/{idProducto}",
                null
            );

            if (respuesta.IsSuccessStatusCode)
                return (respuesta.StatusCode, null);

            string mensaje = await respuesta.Content.ReadAsStringAsync();
            return (respuesta.StatusCode, mensaje);
        }


        // =====================================================================
        //    OBTENER DETALLES DE ARCHIVO
        // =====================================================================
        public async Task<(DetallesArchivoDTO detalles, HttpStatusCode codigo, string mensaje)>
            ObtenerDetallesArchivoAsync(int idProducto, string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            // ✔ CORREGIDO
            var respuesta = await _httpClient.GetAsync($"archivo?idProducto={idProducto}");

            if (respuesta.IsSuccessStatusCode)
            {
                var detalles = await respuesta.Content.ReadFromJsonAsync<DetallesArchivoDTO>();
                return (detalles, respuesta.StatusCode, null);
            }

            string mensaje = await respuesta.Content.ReadAsStringAsync();
            return (null, respuesta.StatusCode, mensaje);
        }


        // =====================================================================
        //    OBTENER IMAGEN
        // =====================================================================
        public async Task<(BitmapImage imagen, HttpStatusCode codigo, string mensaje)> ObtenerImagenAsync(string ruta, string token)
        {
            if (ruta == null)
                return (null, HttpStatusCode.BadRequest, "Ruta nula");

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var respuesta = await _httpClient.GetAsync(ruta);

            if (!respuesta.IsSuccessStatusCode)
            {
                string mensaje = await respuesta.Content.ReadAsStringAsync();
                return (null, respuesta.StatusCode, mensaje);
            }

            var archivo = await respuesta.Content.ReadFromJsonAsync<ArchivoDTO>();

            BitmapImage imagen = ConvertirImagen(archivo.datos);

            if (imagen == null)
                return (null, respuesta.StatusCode, "Error convirtiendo imagen");

            return (imagen, respuesta.StatusCode, null);
        }


        // =====================================================================
        //    CONVERTIR BYTE[] → IMAGEN
        // =====================================================================
        private BitmapImage ConvertirImagen(byte[] datos)
        {
            if (datos == null || datos.Length == 0)
                return null;

            BitmapImage imagen = new BitmapImage();

            using (var ms = new MemoryStream(datos))
            {
                imagen.BeginInit();
                imagen.CacheOption = BitmapCacheOption.OnLoad;
                imagen.StreamSource = ms;
                imagen.EndInit();
                imagen.Freeze();
            }

            return imagen;
        }
    }
}
