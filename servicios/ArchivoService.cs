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

            _httpClient.BaseAddress = new Uri(Constantes.URL);
        }

        public async Task<(DetallesArchivoDTO detalles, HttpStatusCode codigo, string mensaje)>
      ObtenerDetallesArchivoAsync(int idProducto, string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
              new AuthenticationHeaderValue("Bearer", token);

            string endpoint = $"archivo/detalle?idProducto={idProducto}";

            Console.WriteLine("Llamando a: " + _httpClient.BaseAddress + endpoint);

            try
            {
                var respuesta = await _httpClient.GetAsync(endpoint);

                if (respuesta.IsSuccessStatusCode)
                {
                    var detalles = await respuesta.Content.ReadFromJsonAsync<DetallesArchivoDTO>();
                    Console.WriteLine("Datos recibidos. Ruta/ID: " + detalles?.ruta);
                    return (detalles, respuesta.StatusCode, null);
                }

                string mensaje = await respuesta.Content.ReadAsStringAsync();
                Console.WriteLine($"Error obteniendo detalles: {respuesta.StatusCode}");
                return (null, respuesta.StatusCode, mensaje);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Excepción: " + ex.Message);
                return (null, HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<(BitmapImage imagen, HttpStatusCode codigo, string mensaje)>
      ObtenerImagenAsync(string ruta, string token)
        {
            if (string.IsNullOrWhiteSpace(ruta))
                return (null, HttpStatusCode.BadRequest, "Ruta vacía");

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                  new AuthenticationHeaderValue("Bearer", token);

                string baseUrl = Constantes.URL.TrimEnd('/');

                string idArchivo = ruta;

                if (idArchivo.Contains("/"))
                    idArchivo = idArchivo.Substring(idArchivo.LastIndexOf('/') + 1);

                if (idArchivo.Contains("\\"))
                    idArchivo = idArchivo.Substring(idArchivo.LastIndexOf('\\') + 1);

                idArchivo = idArchivo.Trim();

                string urlFinal = $"{baseUrl}/archivo/{idArchivo}";

                Console.WriteLine($" Ruta BD: '{ruta}' -> ID extraído: '{idArchivo}' -> URL: {urlFinal}");

                var respuesta = await _httpClient.GetAsync(urlFinal);

                if (!respuesta.IsSuccessStatusCode)
                {
                    return (null, respuesta.StatusCode, "No se encontró la imagen en el servidor");
                }

                // Para procesar la imagen (JSON o Bytes)
                byte[] datosImagen = null;
                string contentType = respuesta.Content.Headers.ContentType?.MediaType ?? "";

                if (contentType.Contains("json"))
                {
                    try
                    {
                        var archivoDto = await respuesta.Content.ReadFromJsonAsync<ArchivoDTO>();
                        datosImagen = archivoDto?.datos;
                    }
                    catch { }
                }

                if (datosImagen == null || datosImagen.Length == 0)
                {
                    datosImagen = await respuesta.Content.ReadAsByteArrayAsync();
                }

                if (datosImagen != null && datosImagen.Length > 0)
                {
                    var bitmap = ConvertirImagen(datosImagen);
                    return (bitmap, HttpStatusCode.OK, "Ok");
                }

                return (null, HttpStatusCode.NoContent, "Archivo sin datos");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return (null, HttpStatusCode.InternalServerError, ex.Message);
            }
        }

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

        public async Task<(int idArchivo, HttpStatusCode codigo, string mensaje)>
      GuardarArchivoAsync(ArchivoDTO archivo, string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
              new AuthenticationHeaderValue("Bearer", token);

            string json = System.Text.Json.JsonSerializer.Serialize(archivo);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var respuesta = await _httpClient.PostAsync("archivo", content);

            if (!respuesta.IsSuccessStatusCode)
            {
                return (0, respuesta.StatusCode, await respuesta.Content.ReadAsStringAsync());
            }

            string texto = await respuesta.Content.ReadAsStringAsync();

            if (int.TryParse(texto, out int idArchivo))
                return (idArchivo, respuesta.StatusCode, null);

            return (0, respuesta.StatusCode, "Respuesta inesperada: " + texto);
        }


        public async Task<(HttpStatusCode codigo, string mensaje)>
      AsociarArchivoAsync(int idArchivo, int idProducto, string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
              new AuthenticationHeaderValue("Bearer", token);

            var respuesta = await _httpClient.PutAsync(
            $"archivo/asociar/{idArchivo}/{idProducto}",
            null 
            );

            if (respuesta.IsSuccessStatusCode)
                return (respuesta.StatusCode, null);

            string mensaje = await respuesta.Content.ReadAsStringAsync();
            return (respuesta.StatusCode, mensaje);
        }

    }
}