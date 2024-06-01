using System.Text.Json.Serialization;

namespace AG.Dns.Requests.Models
{
    // Represents a unit with symbol, name, and value.
    public class Unit
    {
        [JsonPropertyName("symbol")]
        public required string Symbol { get; set; }

        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("value")]
        public double Value { get; set; }
    }
}
