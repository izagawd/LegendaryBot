namespace Website.Pages.Items;

public partial class Items
{
    public class ItemDto
    {
        public int RarityNum { get; set; }
        public string? Name { get; set; }
        public int Stacks { get; set; }
        public string? ImageUrl { get; set; }




        public int TypeId { get; set; }
    }
}