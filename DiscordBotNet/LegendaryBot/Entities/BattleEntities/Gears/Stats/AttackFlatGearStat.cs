using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears.Stats;

public class AttackFlatGearStat : GearStat
{
    public AttackFlatGearStat()
    {
        TypeId = 1;
    }
    public override int GetMainStatValue(Rarity rarity)
    {
        return ((int) rarity   * 80) + 100;
    }

    public override StatType StatType => StatType.Attack;
    public override bool IsPercentage => false;

    public override void AddStats(Character character)
    {
        character.TotalAttack += Value;
    }



    public override int GetMaximumSubstatLevelIncrease(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.OneStar:
                return 10;
            case Rarity.TwoStar:
                return 20;
            case Rarity.ThreeStar:
                return 30;
            case Rarity.FourStar:
                return 40;
            case Rarity.FiveStar:
                return 50;
            default:
                return 50;
        }
    }

    public override int GetMinimumSubstatLevelIncrease(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.OneStar:
                return 5;
            case Rarity.TwoStar:
                return 10;
            case Rarity.ThreeStar:
                return 15;
            case Rarity.FourStar:
                return 20;
            case Rarity.FiveStar:
                return 25;
            default:
                return 25;
        }
    }
}