using System.ComponentModel;
using BasicFunctionality;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Entities.LegendaryBot;
using Entities.LegendaryBot.Entities.BattleEntities.Blessings;
using Entities.LegendaryBot.Entities.BattleEntities.Characters;
using Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Commands;


public abstract class Banner
{
    public bool IsValidFiveStarBlessing(Type type)
    {
        if (type is null || !type.IsAssignableTo(typeof(Blessing)) || type.IsAbstract)
        {
            return false;
        }

        var def =(Blessing) TypesFunction.GetDefaultObject(type);
        return def.Rarity == Rarity.FiveStar;
    }

    public bool IsValidFiveStarCharacter(Type type)
    {
        if (type is null || !type.IsAssignableTo(typeof(Character)) || type.IsAbstract)
        {
            return false;
        }

        var def =(Character) TypesFunction.GetDefaultObject(type);
        return def.Rarity == Rarity.FiveStar;
    }
    public abstract Type Pull(UserData userData);
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

    public override string Name => $"Limited Blessing Banner " +
                                   $"({((Blessing)TypesFunction.GetDefaultObject(CurrentLimited)).Name})";

    public override Type Pull(UserData userData)
    {
        return GetRandomBannerType(CurrentLimited);
    }

    
    public LimitedBlessingBanner(Type limitedBless)
    {
        if (!IsValidFiveStarBlessing(limitedBless))
        {
            throw new Exception("Invalid blessing type. Needs to be non abstract 5 star blessing");
        }
        CurrentLimited = limitedBless;
    }
}

public class LimitedCharacterBanner : CharacterBanner
{
    public override string Name => $"Limited Character Banner " +
                                   $"({((Character)TypesFunction.GetDefaultObject(CurrentLimited)).Name})";
    public readonly Type CurrentLimited;

    public override Type Pull(UserData userData)
    {
        return GetRandomBannerType(CurrentLimited);
    }

    public LimitedCharacterBanner(Type limitedChar)
    {
        if (!IsValidFiveStarCharacter(limitedChar))
        {
            throw new Exception("Invalid character type. Needs to be non abstract 5 star character");
        }
        CurrentLimited = limitedChar;
    }
}
public abstract class BlessingBanner : Banner
{
        protected Type GetRandomBannerType(Type targetFiveStarBlessing)
    {
        if (!IsValidFiveStarBlessing(targetFiveStarBlessing))
        {
            throw new Exception("Invalid blessing type. Needs to be non abstract five star blessing");
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
  

    protected Type GetRandomBannerType(Type targetFiveStarCharacter)
    {
        if (!IsValidFiveStarCharacter(targetFiveStarCharacter))
        {
            throw new Exception("Invalid character type. Needs to be non abstract 5 star character");
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
    public readonly Banner[] CurrentBanners = [
        new LimitedCharacterBanner(typeof(CommanderJean)),
        new LimitedBlessingBanner(typeof(PowerOfThePhoenix)),
    ];

    [Command("wish")]
    [BotCommandCategory(BotCommandCategory.Battle)]
    [Description("Use this command to pull for characters/blessings!")]
    public async ValueTask WishCommand(CommandContext ctx, [Parameter("banner-number")] int? bannerNumber = null)
    {
        var userData = await DatabaseContext.Set<UserData>()
            .FirstOrDefaultAsync(i => i.DiscordId == ctx.User.Id);
        if (userData is null || userData.Tier == Tier.Unranked)
        {
            await AskToDoBeginAsync(ctx);
            return;
        }

        var builder = new DiscordEmbedBuilder()
            .WithUser(ctx.User)
            .WithColor(userData.Color);
        if (bannerNumber is null)
        {
         
                builder.WithTitle("Current Banners");
            var bannerString = "";
            var count = 1;
            foreach (var i in CurrentBanners)
            {
                bannerString += $"{count++} • {i.Name}\n";
            }

            builder.WithDescription(bannerString);
            await ctx.RespondAsync(builder);
        }
        else
        {
            
            var bannerIndex = bannerNumber - 1;
            if (bannerIndex < 0 || bannerIndex >= CurrentBanners.Length)
            {

                builder.WithTitle("Hmm")
                    .WithDescription($"Banner of numer {bannerNumber} does not exist");
                await ctx.RespondAsync(builder);
                return;
            }
            
        }
    }
}