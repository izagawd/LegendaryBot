using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using PublicInfo;


namespace Website.Pages.Characters;

public partial class CharacterInfo
{
    public class CharacterDto
    {
        public string[] CharacterStatsString { get; set; }
        public long Id { get; set; }

        public int RarityNum { get; set; }
        public int TypeId { get; set; }
        public Dictionary<WorkingWith, long?> TheEquippedOnes { get; set; }
        public int Number { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        [JsonIgnore] public string ImageUrl => $"{PublicInfo.Information.CharactersImagesDirectory}/{TypeId}.png";
    }

    public class GearStatDto
    {
        [JsonIgnore]
        public string StatName { get; internal set; }
        
        [JsonPropertyName("1")]
        public int TypeId { get; set; }
        [JsonPropertyName("2")]
        public bool IsPercentage { get; set; }
        [JsonPropertyName("3")]
        public float Value { get; set; }
        [JsonPropertyName("4")]
        public bool IsMainStat { get; set; }
    }

    public class GearDto
    {
        [JsonIgnore]
        public string GearName { get; internal set; }
        [JsonPropertyName("2")]
        public long Id { get; set; }
        [JsonPropertyName("3")]
        
        public int  RarityNum { get; set; }
        [JsonPropertyName("4")]
        public int Number { get; set; }
        [JsonPropertyName("5")]
        public int TypeId { get; set; }

        [JsonIgnore] public string ImageUrl => $"{Information.GearsImagesDirectory}/{TypeId}.png";
        [JsonPropertyName("8")]
        public string? OriginalOwnerImageUrl { get; set; }
        public GearStatDto[] GearStats { get; set; }

        [JsonIgnore] public IEnumerable<GearStatDto> SubStats => GearStats.Where(i => !i.IsMainStat);
        [JsonIgnore] public GearStatDto MainStat => GearStats.First(i => i.IsMainStat);
    }

    public class BlessingDto{
    
        [JsonPropertyName("1")]
        public int TypeId { get; set; }
        [JsonPropertyName("2")]
        public string Description { get; set; }
        [JsonPropertyName("3")]

        public int RarityNum { get; set; }
        [JsonPropertyName("4")]
        public string Name { get;  set; }
        [JsonIgnore]
        public string  ImageUrl => $"{Information.BlessingImagesDirectory}/{TypeId}.png";
        [JsonPropertyName("6")]
        public int Stacks { get; set; }
        [JsonPropertyName("7")]
        public int RemainingStacks { get; set; }
    }
    public class CharacterInfoDto
    {
        public void Setup()
        {
            foreach (var i in AllGears)
            {
                i.GearName = GearNameByTypeId[i.TypeId];
            }

            foreach (var i in AllGears.SelectMany(j => j.GearStats))
            {
                i.StatName = GearStatNameByTypeId[i.TypeId];
            }

        }
        public  ImmutableDictionary<int,string> GearStatNameByTypeId { get; set; }
        public  ImmutableDictionary<int,string> GearNameByTypeId { get; set; }
         
        public Dictionary<WorkingWith, int> WorkingWithToTypeIdHelper { get; set; }

        public CharacterDto CharacterDto { get; set; }
        public GearDto[] AllGears { get; set; }

        public BlessingDto[] AllBlessings { get; set; }

        public BlessingDto? EquippedBlessing =>
            AllBlessings.FirstOrDefault(i => i.TypeId == CharacterDto.TheEquippedOnes.GetValueOrDefault(WorkingWith.Blessing));



    }
}