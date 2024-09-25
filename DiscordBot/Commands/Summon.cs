using System.ComponentModel;
using BasicFunctionality;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Entities.LegendaryBot;
using Entities.LegendaryBot.Entities;
using Entities.LegendaryBot.Entities.BattleEntities.Blessings;
using Entities.LegendaryBot.Entities.BattleEntities.Characters;
using Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using Entities.LegendaryBot.Entities.Items;
using Entities.LegendaryBot.Rewards;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

namespace DiscordBot.Commands;


public abstract class Banner
{
    public abstract ProbabilityDictionary<Choice> Rates { get; }
    public abstract int FiveStarPityCount { get; }
    public abstract Type SummonsTrackerType { get; }
    public const int FourStarPity = 10;
    public bool IsValidFiveStarBlessing(Type type)
    {
        if (type is null || !type.IsAssignableTo(typeof(Blessing)) || type.IsAbstract)
        {
            return false;
        }

        var def =(Blessing) TypesFunction.GetDefaultObject(type);
        return def.Rarity == Rarity.FiveStar;
    }
    protected Type GetRandomBannerType(Type targetFiveStar, SummonsTracker summonsTracker)
    {
        if ((this is BlessingBanner && !IsValidFiveStarBlessing(targetFiveStar))
            || (this is CharacterBanner && !IsValidFiveStarCharacter(targetFiveStar)))
        {
            throw new Exception($"Invalid type. Needs to be non abstract five star and must fit the banner (in this case, {GetType().Name})");
        }
        if (summonsTracker.FiveStarPity >= FiveStarPityCount -1)
        {
            summonsTracker.FiveStarPity = 0;
            return targetFiveStar;
        }

        var dic = Rates;
        if (summonsTracker.FourStarPity >= FourStarPity - 1)
        {
            dic.Redistribute(Choice.ThreeStarBlessing,[Choice.FourStarBlessing,Choice.FourStarCharacter]);
        }
        var gotten = BasicFunctions.GetRandomFromProbabilities(dic);
     
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
            case Choice.FiveStar:
                gottenType = targetFiveStar;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        var zaObj =(IInventoryEntity) TypesFunction.GetDefaultObject(gottenType);
        summonsTracker.FourStarPity++;
        summonsTracker.FiveStarPity++;
        if (zaObj.Rarity == Rarity.FourStar)
            summonsTracker.FourStarPity = 0;
        if (zaObj.Rarity == Rarity.FiveStar)
            summonsTracker.FiveStarPity = 0;
        return gottenType;
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
    public abstract Type Pull(SummonsTracker summonsTracker);
    public abstract string Name { get; }

}
public enum Choice
{
    FourStarCharacter,  ThreeStarBlessing, FourStarBlessing,
    FiveStar
}


public class LimitedBlessingBanner : BlessingBanner
{
    public readonly Type CurrentLimited;

    public override Type SummonsTrackerType => typeof(LimitedBlessingSummonsTracker);

    public override string Name => $"Limited Blessing Banner " +
                                   $"({((Blessing)TypesFunction.GetDefaultObject(CurrentLimited)).Name})";

    public override Type Pull(SummonsTracker summonsTracker)
    {

        return GetRandomBannerType(CurrentLimited, summonsTracker);
     
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

public class StandardCharacterBanner : CharacterBanner
{
    public IEnumerable<Type> ChoosableCharacters => TypesFunction.GetDefaultObjectsAndSubclasses<Character>()
        .Where(i => i.IsInStandardBanner && i.Rarity == Rarity.FiveStar).Select(i => i.GetType());

    public override Type SummonsTrackerType => typeof(StandardCharacterSummonsTracker);
    public override Type Pull(SummonsTracker summonsTracker)
    {
        if (summonsTracker is not StandardCharacterSummonsTracker standardCharacterSummonsTracker)
            throw new Exception("invalid summons tracker type");
        
        var firstOrDefault =
            ChoosableCharacters
                .FirstOrDefault(i => ((Character)
                    TypesFunction.GetDefaultObject(i)).TypeId ==standardCharacterSummonsTracker.TargetFiveStarTypeId);
        if (firstOrDefault is null)
        {
            throw new Exception(
                $"Character with type id {standardCharacterSummonsTracker.TargetFiveStarTypeId} cannot be pulled in this banner");
        }

        return GetRandomBannerType(firstOrDefault, summonsTracker);
    }

    public override string Name => "Standard Character Banner";
}
public class StandardBlessingBanner : BlessingBanner
{
    public IEnumerable<Type> ChoosableBlessings => TypesFunction.GetDefaultObjectsAndSubclasses<Blessing>()
        .Where(i => i.IsInStandardBanner && i.Rarity == Rarity.FiveStar).Select(i => i.GetType());

    public override Type SummonsTrackerType => typeof(StandardBlessingSummonsTracker);
    public override Type Pull(SummonsTracker summonsTracker)
    {
        if (summonsTracker is not StandardBlessingSummonsTracker standardBlessingSummonsTracker)
            throw new Exception("invalid summons tracker type");
        
        var firstOrDefault =
            ChoosableBlessings
                .FirstOrDefault(i => ((Blessing) TypesFunction.GetDefaultObject(i)).TypeId
                                     ==standardBlessingSummonsTracker.TargetFiveStarTypeId);
        if (firstOrDefault is null)
        {
            throw new Exception(
                $"Blessing with type id {standardBlessingSummonsTracker.TargetFiveStarTypeId} cannot be pulled in this banner");
        }

        return GetRandomBannerType(firstOrDefault, summonsTracker);
    }

    public override string Name => "Standard Blessing Banner";
}
public class LimitedCharacterBanner : CharacterBanner
{


    public override string Name => $"Limited Character Banner " +
                                   $"({((Character)TypesFunction.GetDefaultObject(CurrentLimited)).Name})";
    public readonly Type CurrentLimited;

    public override Type SummonsTrackerType => typeof(LimitedCharacterSummonsTracker);

    public override Type Pull(SummonsTracker summonsTracker)
    {

        return GetRandomBannerType(CurrentLimited, summonsTracker);
   
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
    public override ProbabilityDictionary<Choice> Rates
    {
        get
        {
            return new ProbabilityDictionary<Choice>()
            {
                { Choice.FiveStar, 2 },
                { Choice.FourStarCharacter, 5 },
                { Choice.FourStarBlessing, 7.5 },
                { Choice.ThreeStarBlessing, 85.5 }
            };
        }
    }

    public override int FiveStarPityCount => 60;


}
public abstract class CharacterBanner : Banner
{
    public override ProbabilityDictionary<Choice> Rates
    {
        get
        {
            return new ProbabilityDictionary<Choice>()
            {
                { Choice.FiveStar, 1 },
                { Choice.FourStarCharacter, 5 },
                { Choice.FourStarBlessing, 7.5 },
                { Choice.ThreeStarBlessing, 86.5 }
            };
        }
    }

    public sealed override int FiveStarPityCount => 70;

}
public class Summon : GeneralCommandClass
{
    public readonly Banner[] CurrentBanners = [
        new LimitedCharacterBanner(typeof(CommanderJean)),
        new LimitedBlessingBanner(typeof(HeadStart)),
        new StandardCharacterBanner(),
        new StandardBlessingBanner()
    ];

    [Command("summon")]
    [BotCommandCategory(BotCommandCategory.Battle)]
    [Description("Use this command to pull for characters/blessings!")]
    public async ValueTask WishCommand(CommandContext ctx, 
        [Parameter("banner-number")] int? bannerNumber = null)
    {
        var userData = await DatabaseContext.Set<UserData>()
            .Include(i => i.SummonsTrackers)
            .Include(i => i.Items.Where(j => j is DivineShard))
            .FirstOrDefaultAsync(i => i.DiscordId == ctx.User.Id);
        if (userData is null || userData.Tier == Tier.Unranked)
        {
            await AskToDoBeginAsync(ctx);
            return;
        }

        if (userData.IsOccupied)
        {
            await NotifyAboutOccupiedAsync(ctx);
            return;
        }

       

        var builder = new DiscordEmbedBuilder()
            .WithUser(ctx.User)
            .WithColor(userData.Color);
        (bannerNumber ?? -1).Print();
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
            "yo".Print();
            var bannerIndex = bannerNumber.Value - 1;
            if (bannerIndex < 0 || bannerIndex >= CurrentBanners.Length)
            {

                builder.WithTitle("Hmm")
                    .WithDescription($"Banner of number {bannerNumber} does not exist");
                await ctx.RespondAsync(builder);
                return;
            }

            var banner = CurrentBanners[bannerIndex];
            const int DivineShardsNeeded = 100;
            var divineShards = userData.Items.GetOrCreateItem<DivineShard>();
            builder.WithDescription(
                    $"{DivineShardsNeeded} divine shards per pull" +
                    $" on {banner.Name}. Proceed?")
                .WithFooter($"Note: you have {divineShards.Stacks:N0} divine shards");

            await MakeOccupiedAsync(userData);
            DiscordComponent[] buttonComponents =
            [
                new DiscordButtonComponent(DiscordButtonStyle.Success,
                    "1", "x1"),
                new DiscordButtonComponent(DiscordButtonStyle.Success,
                    "10", "x10"),
                new DiscordButtonComponent(DiscordButtonStyle.Danger,
                    "cancel", "CANCEL"),
            ];
            var typeToLookFor = banner.SummonsTrackerType;
            var gotten =userData.SummonsTrackers.FirstOrDefault(i =>
                i.GetType() == typeToLookFor);
            if (gotten is null)
            {
                gotten =(SummonsTracker) Activator.CreateInstance(typeToLookFor)!;
                if (gotten is StandardCharacterSummonsTracker standardCharacterSummons)
                {
                    standardCharacterSummons.TargetFiveStarTypeId =
                        TypesFunction.GetDefaultObject<StandardCharacterBanner>()
                            .ChoosableCharacters.Select(i => ((Character)TypesFunction.GetDefaultObject(i)).TypeId)
                            .First();
                } else if (gotten is StandardBlessingSummonsTracker standardBlessingSummonsTracker)
                {
                    standardBlessingSummonsTracker.TargetFiveStarTypeId =
                        TypesFunction.GetDefaultObject<StandardBlessingBanner>()
                            .ChoosableBlessings.Select(i => ((Blessing)TypesFunction.GetDefaultObject(i)).TypeId)
                            .First();
                }
                userData.SummonsTrackers.Add(gotten);
            }

       
            const string selectorId = "standard-selector";

            DiscordSelectComponent? createSelectComp()
            {
                if ((banner is StandardBlessingBanner ||banner is  StandardCharacterBanner) 
                    && gotten is StandardSummonsTracker standardSummonsTracker)
                {
             
                    IEnumerable<DiscordSelectComponentOption> options = null!;
                    if (banner is StandardBlessingBanner standardBlessingBanner)
                    {
                        options = standardBlessingBanner.ChoosableBlessings.Select(i =>
                            new DiscordSelectComponentOption(((Blessing)TypesFunction.GetDefaultObject(i)).Name,
                                ((Blessing)TypesFunction.GetDefaultObject(i)).TypeId.ToString(), null,
                                ((Blessing)TypesFunction.GetDefaultObject(i)).TypeId ==
                                standardSummonsTracker.TargetFiveStarTypeId));
                    } else if (banner is StandardCharacterBanner standardCharacterBanner)
                    {
                        options = standardCharacterBanner.ChoosableCharacters.Select(i =>
                            new DiscordSelectComponentOption(((Character)TypesFunction.GetDefaultObject(i)).Name,
                                ((Character)TypesFunction.GetDefaultObject(i)).TypeId.ToString(), null,
                                ((Character)TypesFunction.GetDefaultObject(i)).TypeId ==
                                standardSummonsTracker.TargetFiveStarTypeId));
                    }
                    return new DiscordSelectComponent(selectorId, "Select Target Five Star",options);

                }

                return null;
            }
            var messageBuilder = new DiscordMessageBuilder()
                .AddEmbed(builder)
                .AddComponents(buttonComponents);
            var gottenSelect = createSelectComp();
            if (gottenSelect is not null)
                messageBuilder.AddComponents(gottenSelect);
            await ctx.RespondAsync(messageBuilder);
            var message = (await ctx.GetResponseAsync())!;

            while (true)
            {
                using var tok = new CancellationTokenSource(TimeSpan.FromMinutes(2));
                Task<InteractivityResult<ComponentInteractionCreatedEventArgs>> selectTask = null;
                List<Task<InteractivityResult<ComponentInteractionCreatedEventArgs>>> Tasks = [
                    message.WaitForButtonAsync(ctx.User,tok.Token)];

                if(gottenSelect is not null)
                    Tasks.Add(selectTask = message.WaitForSelectAsync(ctx.User, selectorId, tok.Token));
                var gottenResponse = await Task.WhenAny(Tasks);
                    
                var response = await gottenResponse;
                await tok.CancelAsync();
                if (response.TimedOut)
                {
                    await message.ModifyAsync(i => i.Components
                        .SelectMany(j => j.Components)
                        .ForEach(i => (i as DiscordButtonComponent)?.Disable()));
                    return;
                }

                if (gottenResponse == selectTask)
                {
                    var selectedId = int.Parse(response.Result.Values[0]);
                    if (gotten is StandardSummonsTracker standardSummonsTracker)
                        standardSummonsTracker.TargetFiveStarTypeId = selectedId;
                    await DatabaseContext.SaveChangesAsync();
                    var responseBuilder = new DiscordInteractionResponseBuilder()
                        .AddEmbed(builder)
                        .AddComponents(buttonComponents);
                    gottenSelect = createSelectComp();
                    if (gottenSelect is not null)
                        responseBuilder.AddComponents(gottenSelect);
                    await response.Result.Interaction
                        .CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage,
                            responseBuilder);
                    
                }
                else
                {
                                if (response.Result.Id == "cancel")
            {
                foreach (var i in buttonComponents)
                {
                    (i as DiscordButtonComponent)?.Disable();
                }
                await response.Result.Interaction.CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage,
                    new DiscordInteractionResponseBuilder()
                        .AddEmbed(builder)
                        .AddComponents(buttonComponents));
                return;
            }

            var amount = 1;
            switch (response.Result.Id)
            {
                case "1":
                    amount = 1;
                    break;
                case "10":
                    amount = 10;
                    break;
            }
            
            if (divineShards.Stacks < DivineShardsNeeded * amount)
            {
                builder
                    .WithTitle("hmm")
                    .WithDescription($"You need {DivineShardsNeeded * amount} divine shards to pull x{amount}. You have {divineShards.Stacks:N0}");
                await response.Result.Interaction
                    .CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage,
                        new DiscordInteractionResponseBuilder()
                            .AddEmbed(builder));
                    
                return;
            }

            divineShards.Stacks -= DivineShardsNeeded * amount;
            List<IInventoryEntity> pulledEntities = new List<IInventoryEntity>(amount);

            foreach (var _ in Enumerable.Range(0,amount))
            {
                var gottenType =banner.Pull(gotten);
                IInventoryEntity pulled = (IInventoryEntity)Activator.CreateInstance(gottenType)!;
                if (pulled is null)
                    throw new Exception();
                pulledEntities.Add(pulled);
            }

                
           
            var result =await userData.ReceiveRewardsAsync(DatabaseContext.Set<UserData>(),
                [new EntityReward(pulledEntities)]);
            builder.WithTitle("Nice!")
                .WithFooter($"{divineShards.Stacks:N0} divine shards left")
                .WithDescription(result);
            await DatabaseContext.SaveChangesAsync();
            var responseBuilder = new DiscordInteractionResponseBuilder()
                .AddEmbed(builder)
                .AddComponents(buttonComponents);
            gottenSelect = createSelectComp();
            if (gottenSelect is not null)
                responseBuilder.AddComponents(gottenSelect);
            await response.Result.Interaction
                .CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage,
                    responseBuilder);
                }


            }

        }
    }
}