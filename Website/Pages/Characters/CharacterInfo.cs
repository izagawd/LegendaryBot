using System.Text.Json.Serialization;

namespace Website.Pages.Characters;

public partial class CharacterInfo
{
    public class CharacterDto
    {
        public string Name { get; set; }
        public int Level { get; set; }
        public string ImageUrl { get; set; }
    }

    public class GearStatDto
    {
        public string StatName { get; set; }
        public bool IsPercentage { get; set; }
        public int Value { get; set; }
        public bool IsMainStat { get; set; }
    }

    public class GearDto
    {
        public long Id { get; set; }
        public string GearName { get; set; }

        public string ImageUrl { get; set; }
        public string? OriginalOwnerImageUrl { get; set; }
        public GearStatDto[] GearStats { get; set; }

        [JsonIgnore] public GearStatDto MainStat => GearStats.First(i => i.IsMainStat);
    }

    public class CharacterInfoDto
    {
        public CharacterDto CharacterDto { get; set; }
        public GearDto[] AllGears { get; set; }

        [JsonIgnore]
        public IEnumerable<GearDto> CharacterEquippedGears
        {
            get { return AllGears.Where(j => CharacterEquippedGearsId.Contains(j.Id)); }
        }

        public List<long> CharacterEquippedGearsId { get; set; }
    }
}