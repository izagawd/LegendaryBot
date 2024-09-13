using System.Text.Json.Serialization;

namespace Website.Pages.Gears;

public partial class Gears
{
    public class GearDto
    {
        [JsonPropertyName("1")]
        public int RarityNum { get; set; }
        [JsonPropertyName("2")]
        public string? Name { get; set; }
        [JsonPropertyName("3")]
        public string? ImageUrl { get; set; }
        [JsonPropertyName("4")]
        public int Number { get; set; }

        [JsonPropertyName("5")]

        public int TypeId { get; set; }
    }
}