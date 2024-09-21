using BasicFunctionality;
using Entities.LegendaryBot;
using Entities.LegendaryBot.Entities.BattleEntities.Blessings;
using Entities.LegendaryBot.Entities.BattleEntities.Characters;
using Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using Entities.LegendaryBot.Entities.BattleEntities.Gears;
using Entities.LegendaryBot.Rewards;

namespace DiscordBot.Commands.Region;

public class RogueHaven : Region
{
    public override string Name => "Rogue Haven";

    public override IEnumerable<Type> PossibleEnemies =>
    [
        typeof(Roxy),
        typeof(Takeshi), typeof(CommanderJean)
    ];

    private Gear GenerateGear(Rarity rarity)
    {
        var gear
            = (Gear)Activator.CreateInstance(BasicFunctions.RandomChoice(Gear.AllGearTypes))!;
        gear.Initialize(rarity);
        return gear;
    }
    public override IEnumerable<Reward> GetRewardsAfterBattle(Character main, Tier combatTier)
    {
        switch (combatTier)
        {
            case Tier.Unranked:
            case Tier.Bronze:
                yield return new EntityReward([GenerateGear(Rarity.OneStar), GenerateGear(Rarity.OneStar)]);
                break;
            case Tier.Silver:
                yield return new EntityReward([GenerateGear(Rarity.OneStar), GenerateGear(Rarity.TwoStar)]);
                break;
            case Tier.Gold:
                yield return new EntityReward([GenerateGear(Rarity.TwoStar), GenerateGear(Rarity.ThreeStar)]);
                break;
            case Tier.Platinum:
                yield return new EntityReward([GenerateGear(Rarity.ThreeStar), GenerateGear(Rarity.FourStar)]);
                break;
            case Tier.Diamond:
                yield return new EntityReward([GenerateGear(Rarity.FourStar), GenerateGear(Rarity.FiveStar)]);
                break;
            case Tier.Divine:
                yield return new EntityReward([GenerateGear(Rarity.FiveStar), GenerateGear(Rarity.FiveStar)]);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public override string WhatYouGain => "Gear";

    public override Team GenerateCharacterTeamFor(Type type, out Character main, Tier combatTier)
    {
        main = (Character)Activator.CreateInstance(type)!;
        NonPlayerTeam newTeam = [main];
        switch (main)
        {
            case Roxy:
                main.Blessing = new VitalForce();
                foreach (var i in Enumerable.Range(0,GetSmallFryEnemyCount(combatTier)))
                {
                    var createdChar = new Delinquent();
                    newTeam.Add(createdChar);
                }

                break;
            case Takeshi:
                main.Blessing = new VitalForce();
                break;

            case CommanderJean:
                main.Blessing = new HeadStart();
                foreach (var i in Enumerable.Range(0, GetSmallFryEnemyCount(combatTier)))
                {
                    var createdChar = new Police();
                    newTeam.Add(createdChar);
                }

                break;
        }

        foreach (var i in newTeam) i.SetBotStatsAndLevelBasedOnTier(combatTier);

        return newTeam;
    }
}