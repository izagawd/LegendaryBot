using DiscordBotNet.Extensions;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears.Stats;

public class AttackPercentageGearStat : GearStat
{
    public override int GetMainStatValue(Rarity rarity, int level)
    {
        return (((int) rarity + 1) * (level / 15.0) * 10).Round() + 10;
    }
    
    public override void AddStats(Character character)
    {
        character.TotalAttack += Value * 0.01f * (character.BaseAttack + (character.Blessing?.Attack).GetValueOrDefault(0));
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