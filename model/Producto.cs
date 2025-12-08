using System;
using System.Text.Json.Serialization;
// Asegúrate de usar el namespace correcto según la ubicación de este archivo
// namespace sweet_temptation_clienteEscritorio.model 

public class Producto
{
    // IdProducto, no necesario para el registro POST, pero importante para el modelo
    [JsonPropertyName("idProducto")]
    public int IdProducto { get; set; }

    // Mapeo de C# PascalCase (Nombre) a JSON camelCase o el formato que use tu DTO en la API
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

    [JsonPropertyName("fechaRegistro")]
    public DateTime FechaRegistro { get; set; }

    [JsonPropertyName("fechaModificacion")]
    public DateTime FechaModificacion { get; set; }

    [JsonPropertyName("categoria")]
    public int Categoria { get; set; }
}
