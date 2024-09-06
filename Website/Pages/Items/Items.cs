namespace Website.Pages.Items;

public partial class Items
{
    public class ItemDto
    {
        public int RarityNum { get; set; }
        public string? Name { get; set; } 
        public int Stacks { get; set; }
        public string? ImageUrl { get; set; }


        public string RarityName
        {
            get
            {
                switch (RarityNum)
                {
                    case 1:
                        return "One Star";
                    case 2:
                        return "Two Star";
                    case 3:
                        return "Three Star";
                    case 4:
                        return "Four Star";
                    case 5:
                        return "Five Star";
                    default:
                        return "Unknown Rarity";
                }
            }
        }

        public int TypeId { get; set; }
    }
}