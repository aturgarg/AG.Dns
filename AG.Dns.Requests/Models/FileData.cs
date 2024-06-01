using System.Text.Json.Serialization;

namespace AG.Dns.Requests.Models
{
    // Represents the file data structure for JSON deserialization.
    public class FileData
    {
        [JsonPropertyName("base_symbol")]
        public required string BaseSymbol { get; set; }


        [JsonPropertyName("base_name")]
        public required string BaseName { get; set; }

        [JsonPropertyName("units")]
        public required List<Unit> Units { get; set; }
    }
}
