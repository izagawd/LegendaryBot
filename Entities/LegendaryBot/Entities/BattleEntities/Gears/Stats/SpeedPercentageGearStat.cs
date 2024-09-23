using 
    Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;

namespace Entities.LegendaryBot.Entities.BattleEntities.Gears.Stats;

public class SpeedPercentageGearStat : GearStat
{
    public override int TypeId
    {
        get => 12;
        protected init { }
    }

    public override StatType StatType => StatType.Speed;
    public override bool IsPercentage => true;

    public override int GetMainStatValue(Rarity rarity)
    {
        throw new Exception("Speed percentage should never be a mainstat");
    }

    public override void AddStats(Character character)
    {
        character.TotalSpeed += Value * 0.01f * character.BaseSpeed;
    }

    public override int GetMaximumSubstatLevelIncrease(Rarity rarity)
    {
        throw new Exception("Percentage speed cannot be a main or substat in a gear");
    }

    public override int GetMinimumSubstatLevelIncrease(Rarity rarity)
    {
        throw new Exception("Percentage speed cannot be a main or substat in a gear");
    }
}