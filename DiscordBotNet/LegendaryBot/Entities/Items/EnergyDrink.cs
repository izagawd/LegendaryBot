namespace DiscordBotNet.LegendaryBot.Entities.Items;

public class EnergyDrink : Item
{
    public EnergyDrink()
    {
        TypeId = 4;
    }
    public override Rarity Rarity => Rarity.FourStar;
    const int EnergyToReplenish = 60;

    public override string Description => $"Replenishes {EnergyToReplenish} energy for a user";
}