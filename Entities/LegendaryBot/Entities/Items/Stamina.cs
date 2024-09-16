namespace Entities.LegendaryBot.Entities.Items;

public class Stamina : Energy
{
    public override int TypeId
    {
        get => 7;
        protected init { }
    }

    public override string Description => "Used to explore the wonderful world";

    public override string Name => "Stamina";
    public override TimeSpan EnergyIncreaseInterval => TimeSpan.FromMinutes(5);
    public override int MaxEnergyValue => 240;
}