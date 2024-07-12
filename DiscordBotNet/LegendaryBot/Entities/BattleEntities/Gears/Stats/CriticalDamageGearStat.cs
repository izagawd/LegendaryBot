using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears.Stats;

public class CriticalDamageGearStat  : GearStat
{
    public override int GetMainStatValue(Rarity rarity, int level)
    {
        return (((int) rarity + 1) * (level / 15.0) * 11).Round() + 10;
    }

    public override void AddStats(Character character)
    {
        character.TotalCriticalDamage += Value;
    }

    public override int GetMaximumSubstatLevelIncrease(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.OneStar:
                return 3;
            case Rarity.TwoStar:
                return 4;
            case Rarity.ThreeStar:
                return 5;
            case Rarity.FourStar:
                return 6;
            default:
                return 8;
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