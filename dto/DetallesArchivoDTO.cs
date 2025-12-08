using System.Text.Json.Serialization;

public class DetallesArchivoDTO
{
    [JsonPropertyName("id")]
    public int id { get; set; }

    [JsonPropertyName("fechaRegistro")]
    public DateTime fechaRegistro { get; set; }

    [JsonPropertyName("extension")]
    public string extension { get; set; }

    [JsonPropertyName("ruta")]
    public string ruta { get; set; }
}

