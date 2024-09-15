using System.Text.Json.Serialization;
using PublicInfo;

namespace Website.Pages.Items;

public partial class Items
{
    public class ItemDto
    {
        [JsonPropertyName("1")] public int RarityNum { get; set; }

        [JsonPropertyName("2")] public string? Name { get; set; }

        [JsonPropertyName("3")] public int Stacks { get; set; }

        [JsonIgnore] public string ImageUrl => Information.ItemsImagesDirectory + $"/{TypeId}.png";


        [JsonPropertyName("5")] public int TypeId { get; set; }

        [JsonPropertyName("6")] public string Description { get; set; }
    }
}