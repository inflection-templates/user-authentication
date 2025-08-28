using System.Text.Json.Serialization;

namespace AspNetJwtAuth.Models
{
    public class JwksResponse
    {
        [JsonPropertyName("keys")]
        public List<JsonWebKey> Keys { get; set; } = new();
    }

    public class JsonWebKey
    {
        [JsonPropertyName("kid")]
        public string Kid { get; set; } = string.Empty;

        [JsonPropertyName("kty")]
        public string Kty { get; set; } = string.Empty;

        [JsonPropertyName("use")]
        public string Use { get; set; } = string.Empty;

        [JsonPropertyName("alg")]
        public string Alg { get; set; } = string.Empty;

        [JsonPropertyName("n")]
        public string N { get; set; } = string.Empty;

        [JsonPropertyName("e")]
        public string E { get; set; } = string.Empty;

        [JsonPropertyName("x5c")]
        public string[]? X5c { get; set; }

        [JsonPropertyName("x5t")]
        public string? X5t { get; set; }
    }
}
