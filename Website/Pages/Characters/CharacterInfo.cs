using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;


namespace Website.Pages.Characters;

public partial class CharacterInfo
{
    public class CharacterDto
    {
        public string[] CharacterStatsString { get; set; }
        public long Id { get; set; }

       
        public Dictionary<WorkingWith, long?> TheEquippedOnes { get; set; }
        public int Number { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public string ImageUrl { get; set; }
    }

    public class GearStatDto
    {
        public string StatName { get; set; }
        public bool IsPercentage { get; set; }
        public float Value { get; set; }
        public bool IsMainStat { get; set; }
    }

    public class GearDto
    {
        public string GearName { get; set; }
        public long Id { get; set; }
        public int  RarityNum { get; set; }
        public int Number { get; set; }
        public string RarityName { get; set; }
        public int TypeId { get; set; }
        public string ImageUrl { get; set; }
        public string? OriginalOwnerImageUrl { get; set; }
        public GearStatDto[] GearStats { get; set; }

        [JsonIgnore] public IEnumerable<GearStatDto> SubStats => GearStats.Where(i => !i.IsMainStat);
        [JsonIgnore] public GearStatDto MainStat => GearStats.First(i => i.IsMainStat);
    }

    public class BlessingDto
    {
        public int TypeId { get; set; }
        public string Description { get; set; }
        public string RarityName { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public int Stacks { get; set; }
        public int RemainingStacks { get; set; }
    }
    public class CharacterInfoDto
    {
        
         
        public Dictionary<WorkingWith, int> WorkingWithToTypeIdHelper { get; set; }

        public CharacterDto CharacterDto { get; set; }
        public GearDto[] AllGears { get; set; }

        public BlessingDto[] AllBlessings { get; set; }

        public BlessingDto? EquippedBlessing =>
            AllBlessings.FirstOrDefault(i => i.TypeId == CharacterDto.TheEquippedOnes.GetValueOrDefault(WorkingWith.Blessing));



    }
}