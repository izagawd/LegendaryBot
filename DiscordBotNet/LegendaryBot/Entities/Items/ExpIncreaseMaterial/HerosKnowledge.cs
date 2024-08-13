namespace DiscordBotNet.LegendaryBot.Entities.Items.ExpIncreaseMaterial;

public class HerosKnowledge : CharacterExpMaterial
{
    public override string Name => "Heros Knowledge";

    public override int TypeId
    {
        get => 3;
        protected init {}
    }


    public override Rarity Rarity => Rarity.FourStar;
    public override int ExpToIncrease => 2500;
}