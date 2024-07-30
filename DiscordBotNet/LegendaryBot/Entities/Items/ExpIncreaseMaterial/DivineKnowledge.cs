namespace DiscordBotNet.LegendaryBot.Entities.Items.ExpIncreaseMaterial;

public class DivineKnowledge : CharacterExpMaterial
{
    public override string Name => "Divine Knowledge";

    public DivineKnowledge()
    {
        TypeId = 2;
    }
    public override Rarity Rarity => Rarity.FiveStar;
    public override int ExpToIncrease => 5000;
}