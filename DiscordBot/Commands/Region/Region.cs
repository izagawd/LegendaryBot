using BasicFunctionality;
using Entities.LegendaryBot;
using Entities.LegendaryBot.Entities.BattleEntities.Characters;
using Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using Entities.LegendaryBot.Rewards;

namespace DiscordBot.Commands.Region;

public abstract class Region
{

    public static int GetSmallFryEnemyCount(Tier playerTier)
    {
        switch (playerTier)
        {
            case Tier.Unranked:
                return 0;
            case Tier.Bronze:
                return 1;
            case Tier.Silver:
                return 1;
            case Tier.Gold:
                return 2;
            case Tier.Platinum:
                return 2;
            case Tier.Diamond:
                return 3;
            case Tier.Divine:
                return 3;
            default:
                throw new ArgumentOutOfRangeException(nameof(playerTier), playerTier, null);
        }
    }
    static Region()
    {
        foreach (var i in TypesFunction.GetDefaultObjectsAndSubclasses<Region>())
        {
            var rarities = i.ObtainableCharacters.GroupBy(j =>
                    ((Character)TypesFunction.GetDefaultObject(j)).Rarity)
                .Select(j => j.Key).ToArray();
            if (rarities.Length > 3 || !rarities.Contains(Rarity.ThreeStar) ||
                !rarities.Contains(Rarity.FourStar)
                || !rarities.Contains(Rarity.FiveStar))
                throw new Exception("Each region needs ONLY rarity 3 - 5 star characters");
        }
    }

    public abstract string WhatYouGain { get; }
    public abstract IEnumerable<Reward> GetRewardsAfterBattle(Character main, Tier combatTier);
    public virtual Tier TierRequirement => Tier.Bronze;
    public abstract IEnumerable<Type> ObtainableCharacters { get; }

    public abstract string Name { get; }

    public abstract Team GenerateCharacterTeamFor(Type type, out Character originalCharacter, Tier combatTier);

    public static Region? GetRegion(string regionName)
    {
        var simplifiedRegionName = regionName.ToLower().Replace(" ", "");
        return TypesFunction.GetDefaultObjectsAndSubclasses<Region>()
            .FirstOrDefault(i => i.Name.ToLower().Replace(" ", "")
                                 == simplifiedRegionName);
    }
}