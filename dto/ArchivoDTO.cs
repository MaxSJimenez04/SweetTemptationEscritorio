using System;
using System.Text.Json.Serialization;

namespace sweet_temptation_clienteEscritorio.dto
{
    public class ArchivoDTO
    {
        [JsonPropertyName("id")]
        public int id { get; set; }

        [JsonPropertyName("fechaRegistro")]
        public DateTime? fechaRegistro { get; set; }

        [JsonPropertyName("extension")]
        public string extension { get; set; }

        [JsonPropertyName("datos")]
        public byte[] datos { get; set; }
    }
}
