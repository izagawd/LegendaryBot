using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears.Stats;

public class SpeedFlatGearStat : GearStat
{
    public SpeedFlatGearStat()
    {
        TypeId = 11;
    }
    public override int GetMainStatValue(Rarity rarity)
    {
        return ((int) rarity   * 7) + 10;
    }

    public override StatType StatType => StatType.Speed;
    public override bool IsPercentage => false;
    public override void AddStats(Character character)
    {
        character.TotalSpeed += Value;
    }

    public override int GetMaximumSubstatLevelIncrease(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.OneStar:
                return 1;
            case Rarity.TwoStar:
                return 2;
            case Rarity.ThreeStar:
                return 3;
            default:
                return 4;

        }
    }

    public override int GetMinimumSubstatLevelIncrease(Rarity rarity)
    {
        return 1;
    }
}