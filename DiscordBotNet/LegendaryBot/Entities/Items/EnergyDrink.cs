namespace DiscordBotNet.LegendaryBot.Entities.Items;

public class EnergyDrink : Item
{
    public override int TypeId
    {
        get => 4;
        protected init {}
    }

    public override string Name => "Energy Drink";


    public override Rarity Rarity => Rarity.FourStar;
    const int EnergyToReplenish = 60;

    public override string Description => $"Replenishes {EnergyToReplenish} energy for a user";
}