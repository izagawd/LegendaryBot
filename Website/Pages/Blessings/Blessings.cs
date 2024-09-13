using System.Text.Json.Serialization;

namespace Website.Pages.Blessings;

public partial class Blessings
{
    public class BlessingDto
    {
        public int RarityNum { get; set; }
        public string? Name { get; set; }
        public int Stacks { get; set; }
        public string? ImageUrl { get; set; }


        public int TypeId { get; set; }
    }
}