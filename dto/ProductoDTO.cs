using System;
using System.Text.Json.Serialization;

namespace sweet_temptation_clienteEscritorio.dto
{
    public class ProductoDTO
    {
        [JsonPropertyName("id")]
        public int IdProducto { get; set; }

        [JsonPropertyName("nombre")]
        public string Nombre { get; set; }

        [JsonPropertyName("descripcion")]
        public string Descripcion { get; set; }

        [JsonPropertyName("precio")]
        public decimal Precio { get; set; }

        [JsonPropertyName("disponible")]
        public bool Disponible { get; set; }

        [JsonPropertyName("unidades")]
        public int Unidades { get; set; }

        [JsonPropertyName("categoria")]
        public int Categoria { get; set; }

        [JsonPropertyName("fechaRegistro")]
        public DateTime? FechaRegistro { get; set; }

        [JsonPropertyName("fechaModificacion")]
        public DateTime? FechaModificacion { get; set; }
    }
}
