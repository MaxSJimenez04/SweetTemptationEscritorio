using sweet_temptation_clienteEscritorio.dto;
using sweet_temptation_clienteEscritorio.resources;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
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


        public async Task<(int idArchivo, HttpStatusCode codigo, string mensaje)> GuardarArchivoAsync(ArchivoDTO archivo, string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var respuesta = await _httpClient.PostAsJsonAsync<ArchivoDTO>($"archivo/", archivo);
            if (respuesta.IsSuccessStatusCode)
            {
                var idArchivo = await respuesta.Content.ReadFromJsonAsync<int>();
                return (idArchivo, respuesta.StatusCode, null);
            }
            else
            {
                var mensaje = await respuesta.Content.ReadAsStringAsync();
                return (0, respuesta.StatusCode, mensaje);
            }
        }

        public async Task<(HttpStatusCode codigo, string mensaje)> AsociarArchivoAsync(int idArchivo, int idProducto, string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var respuesta = await _httpClient.PostAsync($"archivo/asociar?idArchivo={idArchivo}&idProducto={idProducto}", null);
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

        public async Task<(DetallesArchivoDTO detalles, HttpStatusCode codigo, string mensaje)> ObtenerDetallesArchivoAsync(int idProducto, string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var respuesta = await _httpClient.GetAsync($"archivo/?idProducto={idProducto}");
            if (respuesta.IsSuccessStatusCode)
            {
                var detalles = await respuesta.Content.ReadFromJsonAsync<DetallesArchivoDTO>();
                return (detalles, respuesta.StatusCode, null);
            }
            else
            {
                string mensaje = await respuesta.Content.ReadAsStringAsync();
                return (null, respuesta.StatusCode, mensaje);
            }
        }

        public async Task<(BitmapImage imagen, HttpStatusCode codigo, string mensaje)> ObtenerImagenAsync(string ruta, string token)
        {
            if (ruta != null)
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var respuesta = await _httpClient.GetAsync(ruta);
                if (respuesta.IsSuccessStatusCode)
                {
                    var archivo = await respuesta.Content.ReadFromJsonAsync<ArchivoDTO>();
                    BitmapImage imagenGenerada = ConvertirImagen(archivo.datos);
                    if(imagenGenerada != null)
                    {
                        return (imagenGenerada, respuesta.StatusCode, null);
                    }
                    else
                    {
                        return(null, respuesta.StatusCode, "Hubo un problema al cargar la imagen");
                    }
                }
                else
                {
                    string mensaje = await respuesta.Content.ReadAsStringAsync();
                    return (null, respuesta.StatusCode, mensaje);
                }
            }
            else
            {
                return (null, HttpStatusCode.BadRequest, "No hay ruta especificada");
            }
            
        }

        private BitmapImage ConvertirImagen(byte[] datos)
        {
            if (datos == null || datos.Length == 0)
            {
                return null;
            }

            var imagen = new BitmapImage();
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
