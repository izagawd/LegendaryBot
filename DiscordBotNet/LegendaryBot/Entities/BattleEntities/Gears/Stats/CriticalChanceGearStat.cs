using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears.Stats;

public class CriticalChanceGearStat : GearStat
{
    public override int GetMainStatValue(Rarity rarity)
    {
        return ((int) rarity  *  9) + 10;
    }

    public override void AddStats(Character character)
    {
        character.TotalCriticalChance += Value;
    }

    public override string Name => "Critical Chance";
    public override bool IsPercentage => true;

    public override int GetMaximumSubstatLevelIncrease(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.OneStar:
                return 2;
            case Rarity.TwoStar:
            case Rarity.ThreeStar:
                return 3;
            case Rarity.FourStar:
                return 4;
            default:
                return 5;
        }
    }

    public override int GetMinimumSubstatLevelIncrease(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.OneStar:
                return 1;
            case Rarity.TwoStar:
            case Rarity.ThreeStar:
            case Rarity.FourStar:
                return 2;
            default:
                return 3;
        }
    }
}