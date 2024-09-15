using System.Text.Json.Serialization;
using PublicInfo;

namespace Website.Pages.Blessings;

public partial class BlessingInfo
{
    public class BlessingDto
    {
        public string Description { get; set; }
        public int TypeId { get; set; }
        public string Name { get; set; }
        public int RarityNum { get; set; }
        [JsonIgnore] public string ImageUrl => $"{Information.BlessingImagesDirectory}/{TypeId}.png";
    }

    public class CharacterDto
    {
        public long Id { get; set; }
        public int Number { get; set; }
        public int TypeId { get; set; }
        public int RarityNum { get; set; }

        public string Name { get; set; }
        [JsonIgnore] public string ImageUrl => $"{Information.CharactersImagesDirectory}/{TypeId}.png";
    }
    public class BlessingInfoDto
    {
        public BlessingDto Blessing { get; set; }
        public CharacterDto[] Characters { get; set; }
    }
}