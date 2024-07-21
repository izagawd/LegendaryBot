namespace DiscordBotNet.LegendaryBot.Entities.Items;

public class EnergyDrink : Item
{
    public override Rarity Rarity => Rarity.FourStar;
    const int EnergyToReplenish = 60;
    public override int TypeId => 4;
    public override string Description => $"Replenishes {EnergyToReplenish} energy for a user";
}