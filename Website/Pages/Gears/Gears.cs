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

        [JsonIgnore] public string? ImageUrl => PublicInfo.Information.GearsImagesDirectory + $"/{TypeId}.png";
        [JsonPropertyName("4")]
        public int Number { get; set; }

        [JsonPropertyName("5")]

        public int TypeId { get; set; }
    }
}