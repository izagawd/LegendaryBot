using System.Text.Json.Serialization;
using PublicInfo;

namespace Website.Pages.Characters;

public partial class Characters
{
    [Serializable]
    public class CharacterDto
    {
        [JsonPropertyName("1")]
        public string Name { get;  set; } = null!;
        [JsonPropertyName("2")]
        public int Number { get; set; }
        [JsonPropertyName("3")]
        public int RarityNum { get; set; }
        [JsonIgnore]
        public string Color
        {
            get
            {
                switch (RarityNum)
                {
                    case 1:
                        return "white";
                    case 2: return "green";
                    case 3: return "blue";
                    case 4: return "purple";
                    default:
                        return "yellow";
                }
            }
        }
        [JsonPropertyName("4")]
        public int TypeId { get; set; }

        [JsonPropertyName("5")] public string ImageUrl => $"{Information.CharactersImagesDirectory}/{TypeId}.png";
        [JsonPropertyName("6")]
        public int Level { get; set; }
    }
}