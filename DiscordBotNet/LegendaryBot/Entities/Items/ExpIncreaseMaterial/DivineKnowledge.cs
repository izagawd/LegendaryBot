namespace DiscordBotNet.LegendaryBot.Entities.Items.ExpIncreaseMaterial;

public class DivineKnowledge : CharacterExpMaterial
{
    public override string Name => "Divine Knowledge";

    public override int TypeId
    {
        get => 2;
        protected init {}
    }

    public override Rarity Rarity => Rarity.FiveStar;
    public override int ExpToIncrease => 5000;
}