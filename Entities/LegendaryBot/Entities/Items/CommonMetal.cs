namespace Entities.LegendaryBot.Entities.Items;




public abstract class Metal : Item
{
    
}
public class CommonMetal : Metal
{
    public override Rarity Rarity => Rarity.OneStar;
    public override string Name => "Common Metal";

    public override int TypeId
    {
        get => 9; protected init{} }
}
public class UncommonMetal : Metal
{
    public override Rarity Rarity => Rarity.TwoStar;
    public override string Name => "Uncommon Metal";

    public override int TypeId
    {
        get => 10; protected init { }
    }
}

public class RareMetal : Metal
{
    public override Rarity Rarity => Rarity.ThreeStar;
    public override string Name => "Rare Metal";

    public override int TypeId
    {
        get => 11; protected init { }
    }
}

public class EpicMetal : Metal
{
    public override Rarity Rarity => Rarity.FourStar;
    public override string Name => "Epic Metal";

    public override int TypeId
    {
        get => 12; protected init { }
    }
}

public class DivineMetal : Metal
{
    public override Rarity Rarity => Rarity.FiveStar;
    public override string Name => "Divine Metal";

    public override int TypeId
    {
        get => 13; protected init { }
    }
}
