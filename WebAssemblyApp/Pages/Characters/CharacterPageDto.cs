namespace WebAssemblyApp.Pages.Characters;
[Serializable]
public class CharacterPageDto
{
    public string Name { get; set; }
    public int Number { get; set; }
    public int RarityNum { get; set; }

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

    public string ImageUrl { get; set; }
    public int Level { get; set; }
}