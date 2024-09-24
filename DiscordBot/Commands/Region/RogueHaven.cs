using BasicFunctionality;
using Entities.LegendaryBot;
using Entities.LegendaryBot.Entities.BattleEntities.Blessings;
using Entities.LegendaryBot.Entities.BattleEntities.Characters;
using Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using Entities.LegendaryBot.Entities.BattleEntities.Gears;
using Entities.LegendaryBot.Entities.Items;
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

    private Item GenerateCraftItem(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.OneStar:
                return new UncommonMetal(){Stacks = 5};
           
            case Rarity.TwoStar:
                return new CommonMetal(){Stacks = 5};
             
            case Rarity.ThreeStar:
                return new RareMetal(){Stacks = 5};
             
            case Rarity.FourStar:
                return new EpicMetal(){Stacks = 5};
            case Rarity.FiveStar:
                return new DivineMetal(){Stacks = 5};
            default:
                throw new ArgumentOutOfRangeException(nameof(rarity), rarity, null);
        }
    }
    public override IEnumerable<Reward> GetRewardsAfterBattle(Character main, Tier combatTier)
    {
        switch (combatTier)
        {
            case Tier.Unranked:
            case Tier.Bronze:
                yield return new EntityReward([GenerateCraftItem(Rarity.OneStar), GenerateCraftItem(Rarity.OneStar)]);
                break;
            case Tier.Silver:
                yield return new EntityReward([GenerateCraftItem(Rarity.OneStar), GenerateCraftItem(Rarity.TwoStar)]);
                break;
            case Tier.Gold:
                yield return new EntityReward([GenerateCraftItem(Rarity.TwoStar), GenerateCraftItem(Rarity.ThreeStar)]);
                break;
            case Tier.Platinum:
                yield return new EntityReward([GenerateCraftItem(Rarity.ThreeStar), GenerateCraftItem(Rarity.FourStar)]);
                break;
            case Tier.Diamond:
                yield return new EntityReward([GenerateCraftItem(Rarity.FourStar), GenerateCraftItem(Rarity.FiveStar)]);
                break;
            case Tier.Divine:
                yield return new EntityReward([GenerateCraftItem(Rarity.FiveStar), GenerateCraftItem(Rarity.FiveStar)]);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public override string WhatYouGain => "Gear Crafting Materials";

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