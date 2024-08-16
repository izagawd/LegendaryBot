namespace DiscordBotNet.LegendaryBot.Entities.Items.ExpIncreaseMaterial;

public class AdventurersKnowledge : CharacterExpMaterial
{
    public override string Name => "Adventurers Knowledge";

    public override int TypeId
    {
        get => 1;
        protected init { }
    }


    public override Rarity Rarity => Rarity.ThreeStar;
    public override int ExpToIncrease => 100;
}