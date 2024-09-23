using 
    Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;

namespace Entities.LegendaryBot.Entities.BattleEntities.Gears.Stats;

public class EffectivenessGearStat : GearStat
{
    public override int TypeId
    {
        get => 7;
        protected init { }
    }

    public override StatType StatType => StatType.Effectiveness;
    public override bool IsPercentage => true;


    public override int GetMainStatValue(Rarity rarity)
    {
        return (int)rarity * 10 + 10;
    }

    public override void AddStats(Character character)
    {
        character.TotalEffectiveness += Value;
    }

    public override int GetMaximumSubstatLevelIncrease(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.OneStar:
                return 4;
            case Rarity.TwoStar:
                return 5;
            case Rarity.ThreeStar:
                return 6;
            case Rarity.FourStar:
                return 7;
            case Rarity.FiveStar:
                return 8;
            default:
                return 4;
        }
    }

    public override int GetMinimumSubstatLevelIncrease(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.OneStar:
                return 2;
            case Rarity.TwoStar:
            case Rarity.ThreeStar:
            case Rarity.FourStar:
                return 3;
            default:
                return 4;
        }
    }
}