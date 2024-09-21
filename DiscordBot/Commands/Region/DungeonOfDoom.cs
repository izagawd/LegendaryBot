using Entities.LegendaryBot;
using Entities.LegendaryBot.Entities.BattleEntities.Characters;
using Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using Entities.LegendaryBot.Entities.Items.ExpIncreaseMaterial;
using Entities.LegendaryBot.Rewards;

namespace DiscordBot.Commands.Region;

public class DungeonOfDoom : Region
{
    public override string WhatYouGain => "Character Exp Materials";
    public override IEnumerable<Reward> GetRewardsAfterBattle(Character main, Tier combatTier)
    {

        switch (combatTier)
        {
            case Tier.Unranked:
            case Tier.Bronze:
                yield return new EntityReward([new AdventurersKnowledge(){Stacks = 3}]);
                break;
            case Tier.Silver:
                yield return new EntityReward([
                    new AdventurersKnowledge() { Stacks = 2 },
                    new HerosKnowledge() { Stacks = 1 }
                ]);
                break;
            case Tier.Gold:
                yield return new EntityReward([
                    new AdventurersKnowledge() { Stacks = 1 },
                    new HerosKnowledge() { Stacks = 2 }
                ]);
                break;
            case Tier.Platinum:
                yield return new EntityReward([
                    new HerosKnowledge() { Stacks = 3 }
                ]);
                break;
            case Tier.Diamond:
                yield return new EntityReward([
                    new HerosKnowledge() { Stacks = 2} ,
                    new DivineKnowledge() { Stacks = 1 }
                ]);
                break;
            case Tier.Divine:
                yield return new EntityReward([
                    new HerosKnowledge() { Stacks = 1} ,
                    new DivineKnowledge() { Stacks = 2 }
                ]);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(combatTier), combatTier, null);
        }
    }


    public override IEnumerable<Type> PossibleEnemies => [typeof(Skeleton)];
    public override string Name => "Dungeon Of Doom";
    public override Team GenerateCharacterTeamFor(Type type, out Character originalCharacter, Tier combatTier)
    {
        var character = (Character)Activator.CreateInstance(type)!;
        NonPlayerTeam team = [character];
        foreach (var _ in Enumerable.Range(0,GetSmallFryEnemyCount(combatTier)))
        {
            team.Add(new Skeleton());
        }

        foreach (var i in team)
        {
            i.SetBotStatsAndLevelBasedOnTier(combatTier);
        }

        originalCharacter = character;
        return team;
    }
}