namespace Entities.LegendaryBot.Entities.Items;

public class EnergyDrink : Item
{
    private const int EnergyToReplenish = 60;

    public override int TypeId
    {
        get => 4;
        protected init { }
    }

    public override string Name => "Energy Drink";


    public override Rarity Rarity => Rarity.FourStar;

    public override string Description => $"Replenishes {EnergyToReplenish} energy for a user";
}