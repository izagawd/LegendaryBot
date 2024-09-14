using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Website.Pages.Gears;

public partial class Gears
{
    public class  GearPageDto
    {


        public void Setup()
        {
            foreach (var i in Gears.SelectMany(i => i.GearStatDtos))
            {
                i.Name = GearStatNameMapper[i.TypeId];
            }
        }
        public ImmutableDictionary<int, string> GearStatNameMapper { get; set; } 
        public GearDto[] Gears { get; set; }
    }
    public class GearStatDto
    {
 
        [JsonIgnore]
        public string Name { get; internal set; }
        
        [JsonPropertyName("1")]
        public float Value { get; set; }
        [JsonPropertyName("2")]
        public int TypeId { get; set; }
        [JsonPropertyName("3")]
        public bool IsMainStat { get; set; }
        
        [JsonPropertyName("4")]
        public bool IsPercentage { get; set; }
    }
    public class GearDto
    {
        [JsonPropertyName("1")]
        public int RarityNum { get; set; }
        [JsonPropertyName("2")]
        public string? Name { get; set; }

        [JsonIgnore] public GearStatDto MainStat => GearStatDtos.First(i => i.IsMainStat);
        [JsonIgnore] public IEnumerable<GearStatDto> Substats => GearStatDtos.Except([MainStat]);
        [JsonIgnore] public string? ImageUrl => PublicInfo.Information.GearsImagesDirectory + $"/{TypeId}.png";
        [JsonPropertyName("4")]
        public int Number { get; set; }

        [JsonPropertyName("5")]

        public int TypeId { get; set; }
        
        [JsonPropertyName("6")]
        
        public GearStatDto[] GearStatDtos { get; set; }
    }
}