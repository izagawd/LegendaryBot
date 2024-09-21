using BasicFunctionality;
using Entities.LegendaryBot;
using Entities.LegendaryBot.Entities.BattleEntities.Blessings;
using Entities.LegendaryBot.Entities.BattleEntities.Characters;
using Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using Entities.LegendaryBot.Entities.Items;
using Entities.LegendaryBot.Rewards;

namespace DiscordBot.Commands.Region;

public class BurgsKingdom : Region
{
    public override string Name => "Burgs Kingdom";

    public override IEnumerable<Type> PossibleEnemies =>
    [
        typeof(RoyalKnight),
        typeof(Blast), typeof(Thana), typeof(Cheerleader)
    ];

    public override string WhatYouGain => "Coins";

    public override IEnumerable<Reward> GetRewardsAfterBattle(Character main, Tier combatTier)
    {
        switch (combatTier)
        {
            case Tier.Unranked:
                break;
            case Tier.Bronze:
                yield return new EntityReward([new Coin(){Stacks = 10000}]);
                break;
            case Tier.Silver:
                yield return new EntityReward([new Coin(){Stacks = 25000}]);
                break;
            case Tier.Gold:
                yield return new EntityReward([new Coin(){Stacks = 40000}]);
                break;
            case Tier.Platinum:
                yield return new EntityReward([new Coin(){Stacks = 55000}]);
                break;
            case Tier.Diamond:
                yield return new EntityReward([new Coin(){Stacks = 70000}]);
                break;
            case Tier.Divine:
                yield return new EntityReward([new Coin(){Stacks = 85000}]);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(combatTier), combatTier, null);
        }
    }

    public override Team GenerateCharacterTeamFor(Type type, out Character main, Tier combatTier)
    {
        main = (Character)Activator.CreateInstance(type)!;
        NonPlayerTeam newTeam = [main];
        switch (main)
        {
            case Blast:
                main.Blessing = new GoingAllOut();
                foreach (var i in Enumerable.Range(0, GetSmallFryEnemyCount(combatTier)))
                {
                    var createdChar = new Slime();
                    newTeam.Add(createdChar);
                }

                break;
            case RoyalKnight:
                main.Blessing = new VitalForce();
                foreach (var i in Enumerable.Range(0, GetSmallFryEnemyCount(combatTier)))
                {
                    var createdChar = new Police();
                    newTeam.Add(createdChar);
                }

                break;
            case Cheerleader:
                main.Blessing = new HeadStart();
                foreach (var i in Enumerable.Range(0,GetSmallFryEnemyCount(combatTier)))
                {
                    var createdChar = new Jock();
                    newTeam.Add(createdChar);
                }

                break;
            case Thana:
                main.Blessing = new HeadStart();
                foreach (var i in Enumerable.Range(0,GetSmallFryEnemyCount(combatTier)))
                {
                    var createdChar = new Skeleton();
                    newTeam.Add(createdChar);
                }

                break;
        }

        foreach (var i in newTeam) i.SetBotStatsAndLevelBasedOnTier(combatTier);

        return newTeam;
    }
}