namespace DiscordBotNet.LegendaryBot.Entities.Items.ExpIncreaseMaterial;

public class AdventurersKnowledge : CharacterExpMaterial
{
    public override string Name => "Adventurers Knowledge";

    public AdventurersKnowledge()
    {
        TypeId = 1;
    }

    public override Rarity Rarity => Rarity.ThreeStar;
    public override int ExpToIncrease => 100;
    
    
}