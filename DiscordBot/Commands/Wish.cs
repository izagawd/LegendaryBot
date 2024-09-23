using BasicFunctionality;
using Entities.LegendaryBot;
using Entities.LegendaryBot.Entities.BattleEntities.Blessings;
using Entities.LegendaryBot.Entities.BattleEntities.Characters;
using Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;

namespace DiscordBot.Commands;


public abstract class Banner
{
    
    public abstract string Name { get; }

}
public enum Choice
{
    FourStarCharacter, FiveStarCharacter, ThreeStarBlessing, FourStarBlessing,
    FiveStarBlessing
}

public class LimitedBlessingBanner : CharacterBanner
{
    public readonly Type CurrentLimited;

    public Type Pull()
    {
        return GetRandomBannerType(CurrentLimited);
    }

    public LimitedBlessingBanner(Type limitedBless)
    {
        if (limitedBless is null || !limitedBless.IsAssignableTo(typeof(Blessing)) || limitedBless.IsAbstract)
        {
            throw new Exception("Invalid blessing type");
        }
        CurrentLimited = limitedBless;
    }
}

public class LimitedCharacterBanner : CharacterBanner
{
    public readonly Type CurrentLimited;

    public Type Pull()
    {
        return GetRandomBannerType(CurrentLimited);
    }

    public LimitedCharacterBanner(Type limitedChar)
    {
        if (limitedChar is null || !limitedChar.IsAssignableTo(typeof(Character)) || limitedChar.IsAbstract)
        {
            throw new Exception("Invalid character type");
        }
        CurrentLimited = limitedChar;
    }
}
public abstract class BlessingBanner : Banner
{
        protected Type GetRandomBannerType(Type targetFiveStarBlessing)
    {
        if (targetFiveStarBlessing is null || !targetFiveStarBlessing.IsAssignableTo(typeof(Blessing)) || targetFiveStarBlessing.IsAbstract)
        {
            throw new Exception("Invalid blessing type");
        }
        var gotten = BasicFunctions.GetRandom( new Dictionary<Choice, double>()
            {
                { Choice.FiveStarBlessing ,2},
                { Choice.FourStarCharacter , 5},
                { Choice.FourStarBlessing , 7.5},
                { Choice.ThreeStarBlessing ,85.5}
            }
        );
        Type gottenType;
        var charactersToWorkWith = TypesFunction
            .GetDefaultObjectsAndSubclasses<Character>()
            .Where(i => i.IsInStandardBanner && !i.GetType().IsAbstract)
            .ToArray();
        var blessingsToWorkWith = TypesFunction
            .GetDefaultObjectsAndSubclasses<Blessing>()
            .Where(i => i.IsInStandardBanner && !i.GetType().IsAbstract)
            .ToArray();
        switch (gotten)
        {
            case Choice.FourStarCharacter:
                gottenType = BasicFunctions.RandomChoice(
                    charactersToWorkWith.Where(i => i.Rarity == Rarity.FourStar)
                        .Select(i => i.GetType()));
                    
                break;
            case Choice.ThreeStarBlessing:
                gottenType = BasicFunctions.RandomChoice(
                    blessingsToWorkWith.Where(i => i.Rarity == Rarity.ThreeStar)
                        .Select(i => i.GetType()));
                break;
            case Choice.FourStarBlessing:
                gottenType = BasicFunctions.RandomChoice(
                    blessingsToWorkWith.Where(i => i.Rarity == Rarity.FourStar)
                        .Select(i => i.GetType()));
                
                break;
            case Choice.FiveStarBlessing:
                gottenType = targetFiveStarBlessing;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return gottenType;
    }
}
public abstract class CharacterBanner : Banner
{
    public override string Name => "Standard Banner";

    protected Type GetRandomBannerType(Type targetFiveStarCharacter)
    {
        if (targetFiveStarCharacter is null || !targetFiveStarCharacter.IsAssignableTo(typeof(Character)) || targetFiveStarCharacter.IsAbstract)
        {
            throw new Exception();
        }
        var gotten = BasicFunctions.GetRandom( new Dictionary<Choice, double>()
            {
                { Choice.FiveStarCharacter ,1},
                { Choice.FourStarCharacter , 5},
                { Choice.FourStarBlessing , 7.5},
                { Choice.ThreeStarBlessing ,86.5}
            }
        );
        Type gottenType;
        var charactersToWorkWith = TypesFunction
            .GetDefaultObjectsAndSubclasses<Character>()
            .Where(i => i.IsInStandardBanner && !i.GetType().IsAbstract)
            .ToArray();
        var blessingsToWorkWith = TypesFunction
            .GetDefaultObjectsAndSubclasses<Blessing>()
            .Where(i => i.IsInStandardBanner && !i.GetType().IsAbstract)
            .ToArray();
        switch (gotten)
        {
            case Choice.FourStarCharacter:
                gottenType = BasicFunctions.RandomChoice(
                    charactersToWorkWith.Where(i => i.Rarity == Rarity.FourStar)
                        .Select(i => i.GetType()));
                    
                break;
            case Choice.FiveStarCharacter:
                gottenType = targetFiveStarCharacter;
                break;
            case Choice.ThreeStarBlessing:
                gottenType = BasicFunctions.RandomChoice(
                    blessingsToWorkWith.Where(i => i.Rarity == Rarity.ThreeStar)
                        .Select(i => i.GetType()));
                break;
            case Choice.FourStarBlessing:
                gottenType = BasicFunctions.RandomChoice(
                    blessingsToWorkWith.Where(i => i.Rarity == Rarity.FourStar)
                        .Select(i => i.GetType()));
                
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return gottenType;
    }
}
public class Wish : GeneralCommandClass
{
    
}