using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;

namespace DiscordBotNet.LegendaryBot.Commands;

public abstract class Region
{
    public virtual Tier TierRequirement => Tier.Bronze;
    public abstract IEnumerable<Type> ObtainableCharacters { get; }

    public virtual string Name => GetType().Name;

    static Region()
    {
        foreach (var i in TypesFunctionality.GetDefaultObjectsThatIsInstanceOf<Region>())
        {
            var rarities = i.ObtainableCharacters.GroupBy(j =>
                    ((Character)TypesFunctionality.GetDefaultObject(j)).Rarity)
                .Select(j => j.Key).ToArray();
            if (!rarities.Contains(Rarity.TwoStar)
                || !rarities.Contains(Rarity.ThreeStar) ||
                !rarities.Contains(Rarity.FourStar)
                || !rarities.Contains(Rarity.FiveStar))
            {
                throw new Exception("Each region needs rarity 2 - 5 star characters");
            }

        }
    }
    public static Region? GetRegion(string regionName)
    {
        var simplifiedRegionName = regionName.ToLower().Replace(" ", "");
        return TypesFunctionality.GetDefaultObjectsThatIsInstanceOf<Region>()
            .FirstOrDefault(i => i.Name.ToLower().Replace(" ", "")
                        == simplifiedRegionName);
    }
}