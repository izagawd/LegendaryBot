using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears.Stats;

public class ResistanceGearStat : GearStat
{
    public ResistanceGearStat()
    {
        TypeId = 10;
    }
    public override int GetMainStatValue(Rarity rarity)
    {
        return ((int) rarity * 10) + 10;
    }

    public override void AddStats(Character character)
    {
        character.TotalResistance += Value;
    }

    public override StatType StatType => StatType.Resistance;
    public override bool IsPercentage => true;
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