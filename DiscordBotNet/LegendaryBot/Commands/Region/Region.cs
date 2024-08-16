using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;

namespace DiscordBotNet.LegendaryBot.Commands;

public abstract class Region
{
    public virtual Tier TierRequirement => Tier.Bronze;
    public abstract IEnumerable<Type> ObtainableCharacters { get; }

    public abstract string Name { get; }

    public abstract CharacterTeam GenerateCharacterTeamFor(Type type, out Character originalCharacter);
   
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
            {
                throw new Exception("Each region needs ONLY rarity 3 - 5 star characters");
            } 
   
        }
    }
    public static Region? GetRegion(string regionName)
    {
        var simplifiedRegionName = regionName.ToLower().Replace(" ", "");
        return TypesFunction.GetDefaultObjectsAndSubclasses<Region>()
            .FirstOrDefault(i => i.Name.ToLower().Replace(" ", "")
                        == simplifiedRegionName);
    }
}