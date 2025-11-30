using sweet_temptation_clienteEscritorio.dto;
using sweet_temptation_clienteEscritorio.resources;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace sweet_temptation_clienteEscritorio.servicios
{
    public class UsuarioService
    {
        private readonly HttpClient _httpClient;

        public UsuarioService()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(Constantes.URL);

            var token = App.Current.Properties["Token"]?.ToString();
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<(List<UserResponseDTO>? usuarios, HttpStatusCode codigo, string? mensaje)> GetUsuariosAsync()
        {
            try
            {
                var respuesta = await _httpClient.GetAsync("api/usuarios");

                if (respuesta.IsSuccessStatusCode)
                {
                    var usuarios = await respuesta.Content.ReadFromJsonAsync<List<UserResponseDTO>>();
                    return (usuarios, respuesta.StatusCode, null);
                }
                else
                {
                    string mensaje = await respuesta.Content.ReadAsStringAsync();
                    return (null, respuesta.StatusCode, mensaje);
                }
            }
            catch (Exception ex)
            {
                return (null, HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<(UserResponseDTO? usuario, HttpStatusCode codigo, string? mensaje)> GetUsuarioByIdAsync(int id)
        {
            try
            {
                var respuesta = await _httpClient.GetAsync($"api/usuarios/{id}");

                if (respuesta.IsSuccessStatusCode)
                {
                    var usuario = await respuesta.Content.ReadFromJsonAsync<UserResponseDTO>();
                    return (usuario, respuesta.StatusCode, null);
                }
                else
                {
                    string mensaje = await respuesta.Content.ReadAsStringAsync();
                    return (null, respuesta.StatusCode, mensaje);
                }
            }
            catch (Exception ex)
            {
                return (null, HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<(UserResponseDTO? usuario, HttpStatusCode codigo, string? mensaje)> CreateUsuarioAsync(UserRequestDTO request)
        {
            try
            {
                var respuesta = await _httpClient.PostAsJsonAsync("api/usuarios", request);

                if (respuesta.IsSuccessStatusCode)
                {
                    var usuario = await respuesta.Content.ReadFromJsonAsync<UserResponseDTO>();
                    return (usuario, respuesta.StatusCode, null);
                }
                else
                {
                    string mensaje = await respuesta.Content.ReadAsStringAsync();
                    return (null, respuesta.StatusCode, mensaje);
                }
            }
            catch (Exception ex)
            {
                return (null, HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<(UserResponseDTO? usuario, HttpStatusCode codigo, string? mensaje)> UpdateUsuarioAsync(int id, UserRequestDTO request)
        {
            try
            {
                var respuesta = await _httpClient.PutAsJsonAsync($"api/usuarios/{id}", request);

                if (respuesta.IsSuccessStatusCode)
                {
                    var usuario = await respuesta.Content.ReadFromJsonAsync<UserResponseDTO>();
                    return (usuario, respuesta.StatusCode, null);
                }
                else
                {
                    string mensaje = await respuesta.Content.ReadAsStringAsync();
                    return (null, respuesta.StatusCode, mensaje);
                }
            }
            catch (Exception ex)
            {
                return (null, HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<(HttpStatusCode codigo, string? mensaje)> DeleteUsuarioAsync(int id)
        {
            try
            {
                var respuesta = await _httpClient.DeleteAsync($"api/usuarios/{id}");

                if (respuesta.IsSuccessStatusCode)
                {
                    return (respuesta.StatusCode, null);
                }
                else
                {
                    string mensaje = await respuesta.Content.ReadAsStringAsync();
                    return (respuesta.StatusCode, mensaje);
                }
            }
            catch (Exception ex)
            {
                return (HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
