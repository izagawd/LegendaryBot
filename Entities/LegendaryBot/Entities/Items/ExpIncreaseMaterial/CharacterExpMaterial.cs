namespace Entities.LegendaryBot.Entities.Items.ExpIncreaseMaterial;

public abstract class CharacterExpMaterial : Item
{
    public override string Description => "Used to level up characters";
    public virtual int ExpToIncrease => 0;
}