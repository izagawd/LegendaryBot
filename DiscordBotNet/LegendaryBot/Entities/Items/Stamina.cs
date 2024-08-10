namespace DiscordBotNet.LegendaryBot.Entities.Items;

public class Stamina : Energy
{
    public override string Name { get; }
    public override TimeSpan EnergyIncreaseInterval => TimeSpan.FromMinutes(5);
    public override int MaxEnergyValue => 240;
}