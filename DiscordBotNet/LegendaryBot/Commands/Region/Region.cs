namespace DiscordBotNet.LegendaryBot.Commands;

public abstract class Region
{
    public abstract IEnumerable<Type> ObtainableCharacters { get; }
    public Region()
    {
        Name = GetType().Name.Englishify();
    }
    public string Name { get; }

    public static Region? GetRegion(string regionName)
    {
        var simplifiedRegionName = regionName.ToLower().Replace(" ", "");
        return TypesFunctionality.GetDefaultObjectsThatIsInstanceOf<Region>()
            .FirstOrDefault(i => i.Name.ToLower().Replace(" ", "")
                        == simplifiedRegionName);
    }
}