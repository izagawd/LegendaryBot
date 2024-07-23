namespace DiscordBotNet.LegendaryBot.Entities.Items.ExpIncreaseMaterial;

public class HerosKnowledge : CharacterExpMaterial
{
    public HerosKnowledge()
    {
        TypeId = 3;
    }

    public override Rarity Rarity => Rarity.FourStar;
    public override int ExpToIncrease => 2500;
}