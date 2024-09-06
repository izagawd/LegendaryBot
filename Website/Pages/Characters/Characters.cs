using System.Text.Json.Serialization;

namespace Website.Pages.Characters;

public partial  class Characters
{
    [Serializable]
    public class CharacterDto
    {
        public string Name { get; set; } = null!;
        public int Number { get; set; }
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

        public string ImageUrl { get; set; } = null!;
        public int Level { get; set; }
    }
}