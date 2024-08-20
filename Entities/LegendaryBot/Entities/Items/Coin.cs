namespace Entities.LegendaryBot.Entities.Items;

public class Coin : Item
{
    public override int TypeId
    {
        get => 5;
        protected init { }
    }


    public override string Name => "Coin";
}