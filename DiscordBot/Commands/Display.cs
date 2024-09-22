using System.ComponentModel;
using BasicFunctionality;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using Entities.LegendaryBot;
using Entities.LegendaryBot.Entities;
using Entities.LegendaryBot.Entities.BattleEntities.Blessings;
using Entities.LegendaryBot.Entities.BattleEntities.Characters;
using Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using Entities.LegendaryBot.Entities.BattleEntities.Gears;
using Entities.LegendaryBot.Entities.Items;
using Entities.LegendaryBot.Entities.Items.ExpIncreaseMaterial;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace DiscordBot.Commands;

[Command("display")]
public class Display : GeneralCommandClass
{
    protected static DiscordButtonComponent Next = new(DiscordButtonStyle.Success, "next", "NEXT");
    protected static DiscordButtonComponent Previous = new(DiscordButtonStyle.Primary, "previous", "PREVIOUS");

// Last Button
    protected static DiscordButtonComponent Last = new(DiscordButtonStyle.Primary, "last", "LAST");

    private static readonly List<string> entitiesList;
    protected static DiscordButtonComponent First = new(DiscordButtonStyle.Primary, "first", "FIRST");

    static Display()
    {
        entitiesList = [];
        foreach (var i in TypesFunction.GetDefaultObjectsAndSubclasses<IInventoryEntity>()
                     .Select(i =>
                     {
                         Type type;
                         if (i is Character)
                             type = typeof(Character);
                         else if (i is Gear)
                             type = typeof(Gear);
                         else if (i is CharacterExpMaterial)
                             type = typeof(CharacterExpMaterial);
                         else if (i is Item)
                             type = typeof(Item);
                         else if (i is Blessing)
                             type = typeof(Blessing);
                         else
                             type = typeof(IInventoryEntity);
                         return new { type, entity = i };
                     }).OrderBy(i => i.type.Name))
            entitiesList.Add($"{i.entity.Name} ({i.type.Name})");
    }


    private static async Task<Image<Rgba32>> GetCharacterDisplayAsync(Character character)
    {
        var image = new Image<Rgba32>(450, 300);
        using var characterImage = await ImageFunctions.GetImageFromUrlAsync(character.ImageUrl);
        characterImage.Mutate(i => i.Resize(75, 75));
        image.Mutate(i => i.DrawImage(characterImage,
            new Point(200, 100), new GraphicsOptions()));
        return image;
    }

    [Command("character-gear")]
    [Description("Displays a character's gears")]
    [BotCommandCategory(BotCommandCategory.Character)]
    public async ValueTask ExecuteDisplayCharacterGear(CommandContext context,
        [Parameter("character-name")] string characterName)
    {
        var typeIdToLookFor = Character.LookFor(characterName);
        var userData = await DatabaseContext.Set<UserData>()
            .AsNoTrackingWithIdentityResolution()
            .Include(i => i.Characters
                .Where(j => j.TypeId == typeIdToLookFor))
            .ThenInclude(j => j.Gears)
            .ThenInclude(j => j.Stats)
            .Include(i => i.Characters)
            .ThenInclude(i => i.Blessing)
            .FirstOrDefaultAsync(i => i.DiscordId == context.User.Id);
        if (userData is null || userData.Tier == Tier.Unranked)
        {
            await AskToDoBeginAsync(context);
            return;
        }

        var embed = new DiscordEmbedBuilder()
            .WithTitle("Gears")
            .WithUser(context.User)
            .WithColor(userData.Color);
        var character = userData.Characters.FirstOrDefault();
        if (character is null)
        {
            embed.WithDescription($"Character with number {characterName} not found");
            await context.RespondAsync(embed);
            return;
        }

        var count = 0;
        embed.WithTitle($"{character.Name}'s gears");
        foreach (var i in character.Gears.OrderBy(i => i.Name))
        {
            embed.AddField(i.Name, i.DisplayString, true);
            count++;
            if (count == 2)
            {
                embed.AddField("\u200b", "\u200b");
                count = 0;
            }
        }


        var messageBuilder = new DiscordMessageBuilder()
            .AddEmbed(embed);
        await context.RespondAsync(messageBuilder);
    }

    private static async ValueTask ExecuteDisplayAsync(CommandContext context, IEnumerable<string> objects,
        int displaySectionLimit,
        string joiner, string title,
        DiscordColor discordColor)
    {
        List<List<string>> displayList = [];
        var count = 0;
        List<string> currentList = [];

        displayList.Add(currentList);

        foreach (var i in objects)
        {
            if (count >= displaySectionLimit)
            {
                currentList = new List<string>();
                displayList.Add(currentList);
                count = 0;
            }

            currentList.Add(i);
            count++;
        }

        var index = 0;

        DiscordMessage? message = null;


        while (true)
        {
            var embed = new DiscordEmbedBuilder()
                .WithUser(context.User)
                .WithColor(discordColor)
                .WithTitle($"{title} (Page {index + 1}/{displayList.Count})")
                .WithDescription(displayList[index].Join(joiner));
            var messageBuilder = new DiscordMessageBuilder()
                .AddComponents(First, Previous, Next, Last)
                .AddEmbed(embed);

            if (message is null)
            {
                var response = new DiscordInteractionResponseBuilder(messageBuilder);
                await context.RespondAsync(response);
                message = await context.GetResponseAsync();
            }
            else
            {
                message = await message.ModifyAsync(messageBuilder);
            }

            var result = await message!.WaitForButtonAsync(context.User);

            if (result.TimedOut) break;
            await result.Result.Interaction.CreateResponseAsync(DiscordInteractionResponseType.DeferredMessageUpdate);
            switch (result.Result.Id.ToLower())
            {
                case "next":
                    index++;
                    break;
                case "previous":
                    index--;
                    break;
                case "last":
                    index = displayList.Count - 1;
                    break;
                case "first":
                    index = 0;
                    break;
            }

            if (index < 0) index = 0;
            if (index > displayList.Count - 1) index = displayList.Count - 1;
        }
    }

    [Command("items")]
    [Description("Displays all the stackable items you have")]
    [BotCommandCategory(BotCommandCategory.Inventory)]
    public async ValueTask ExecuteDisplayItems(CommandContext context,
        [Description("pretty self explanatory")]
        string nameFilter = "")
    {
        var simplified = nameFilter.Replace(" ", "").ToLower();
        var userData = await DatabaseContext.Set<UserData>()
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.DiscordId == context.User.Id);

        if (userData is null || userData.Tier == Tier.Unranked)
        {
            await AskToDoBeginAsync(context);
            return;
        }

        foreach (var i in userData.Items.OfType<Energy>())
        {
            i.RefreshEnergyValue();
        }

        await ExecuteDisplayAsync(context, userData.Items.Where(i => i.Name
                    .Replace(" ", "")
                    .Contains(simplified, StringComparison.CurrentCultureIgnoreCase))
                .Select(i => $"Name • {i.Name} • Stacks • {i.Stacks:N0}"), 20,
            "\n", "Items", userData.Color);
    }

    [Command("all-entities")]
    [Description("Displays all entities that can be gotten into your inventory")]
    [BotCommandCategory]
    public async ValueTask ExecuteDisplayAllEntities(CommandContext context)
    {
        var color = (await DatabaseContext.Set<UserData>()
                .Where(i => i.DiscordId == context.User.Id)
                .Select(i => new DiscordColor?(i.Color))
                .FirstOrDefaultAsync())
            .GetValueOrDefault(TypesFunction.GetDefaultObject<UserData>().Color);
        await ExecuteDisplayAsync(context, entitiesList, 15,
            "\n", "All entities", color);
    }

    [Command("gears")]
    [Description("Displays all the gears you have")]
    [BotCommandCategory(BotCommandCategory.Inventory)]
    public async ValueTask ExecuteDisplayGears(CommandContext context)
    {
        var userData = await DatabaseContext.Set<UserData>()
            .AsNoTrackingWithIdentityResolution()
            .Include(i => i.Gears)
            .ThenInclude(i => i.Character)
            .Include(i => i.Gears)
            .ThenInclude(i => i.Stats)
            .FirstOrDefaultAsync(i => i.DiscordId == context.User.Id);
        if (userData is null || userData.Tier == Tier.Unranked)
        {
            await AskToDoBeginAsync(context);
            return;
        }

        await ExecuteDisplayAsync(context, userData.Gears.OrderBy(i => i.Number).Select(i => i.DisplayString), 3,
            "\n\n", "Gears", userData.Color);
    }

    [Command("characters")]
    [Description("Displays all the characters you have")]
    [BotCommandCategory(BotCommandCategory.Character)]
    public async ValueTask ExecuteDisplayCharacters(CommandContext context,
        [Description("pretty self explanatory")]
        string nameFilter = "")
    {
        var simplified = nameFilter.Replace(" ", "").ToLower();
        var userData = await DatabaseContext.Set<UserData>()
            .Include(i => i.Characters)
            .ThenInclude(i => i.Blessing)
            .FirstOrDefaultAsync(i => i.DiscordId == context.User.Id);

        if (userData is null || userData.Tier == Tier.Unranked)
        {
            await AskToDoBeginAsync(context);
            return;
        }

        await ExecuteDisplayAsync(context, userData.Characters
                .Where(i => i.Name.ToLower().Replace(" ", "").Contains(simplified))
                .OrderByDescending(i => i.Rarity)
                .Select(i =>
                {
                    var toReturn = $"{i.Name} • Lvl {i.Level}";
                    if (i.Blessing is not null)
                        toReturn += $" • {i.Blessing.Name}";
                    return toReturn;
                }), 10,
            "\n", "Characters", userData.Color);
    }

    [Command("blessings")]
    [Description("Displays all the blessings you have")]
    [BotCommandCategory(BotCommandCategory.Inventory)]
    public async ValueTask ExecuteDisplayBlessings(CommandContext context, string nameFilter = "")
    {
        var userData = await DatabaseContext.Set<UserData>()
            .Include(i => i.Blessings)
            .FirstOrDefaultAsync(i => i.DiscordId == context.User.Id);
        if (userData is null || userData.Tier == Tier.Unranked)
        {
            await AskToDoBeginAsync(context);
            return;
        }

        var simplified = nameFilter.ToLower().Replace(" ", "");
        await ExecuteDisplayAsync(context, userData.Blessings
                .Where(i => i.Name.ToLower().Replace(" ", "").Contains(simplified))
                .GroupBy(i => i.TypeId)
                .Select(i =>
                {
                    var asArray = i.ToArray();
                    var count = asArray.Length;
                    var countThatsFree = asArray.Count(j => j.CharacterId is null);
                    var sample = asArray[0];
                    return $"`{sample.Name} • Count: {count} • Available: {countThatsFree}`";
                }), 10,
            "\n\n", "Blessings", userData.Color);
    }

    [Command("gear")]
    [Description("Displays a gear based on provided number")]
    [BotCommandCategory(BotCommandCategory.Inventory)]
    public async ValueTask ExecuteDisplayGearByNum(CommandContext context, [Parameter("gear-num")] int gearNumber)
    {
        var userData = await DatabaseContext.Set<UserData>()
            .Include(i => i.Gears.Where(j => j.Number == gearNumber))
            .ThenInclude(i => i.Stats)
            .Include(i => i.Gears)
            .ThenInclude(i => i.Character)
            .FirstOrDefaultAsync(i => i.DiscordId == context.User.Id);
        if (userData is null || userData.Tier == Tier.Unranked)
        {
            await AskToDoBeginAsync(context);
            return;
        }


        var gear = userData.Gears.FirstOrDefault(i => i.Number == gearNumber);
        var embed = new DiscordEmbedBuilder()
            .WithColor(userData.Color)
            .WithTitle("Displaying gear")
            .WithUser(context.User);
        if (gear is null)
            embed.WithDescription($"Gear with number {gearNumber} not found");
        else
            embed.WithDescription(gear.DisplayString);

        await context.RespondAsync(embed);
    }

    [Command("teams")]
    [BotCommandCategory(BotCommandCategory.Team)]
    [Description("Displays all the teams you have and the characters in them")]
    public async ValueTask ExecuteDisplayTeams(CommandContext context)
    {
        var userData = await DatabaseContext.Set<UserData>()
            .Include(i => i.PlayerTeams)
            .ThenInclude(i => i.TeamMemberships)
            .ThenInclude(i => i.Character)
            .FirstOrDefaultAsync(i => i.DiscordId == context.User.Id);
        if (userData is null || userData.Tier == Tier.Unranked)
        {
            await AskToDoBeginAsync(context);
            return;
        }


        var embed = new DiscordEmbedBuilder()
            .WithUser(context.User)
            .WithTitle("Here's a list of all your teams")
            .WithColor(userData.Color)
            .WithDescription("");
        foreach (var i in userData.PlayerTeams)
        {
            var equipped = "";
            var value = "NO TEAM MEMBER";
            if (userData.EquippedPlayerTeam == i)
                equipped = " (equipped)";

            value = "";
            var maxChar = TypesFunction.GetDefaultObject<PlayerTeam>().MaxCharacters;
            for (var j = 1; j <= maxChar; j++)
            {
                value += $"`Slot {j}: ";
                var membership = i.TeamMemberships.FirstOrDefault(k => k.Slot == j);
                if (membership is null)
                    value += "EMPTY SLOT";
                else
                    value +=
                        $"{membership.Character.Name} • Lvl {membership.Character.Level}";

                value += "`\n";
            }


            embed.AddField(i.TeamName + equipped,
                value);
        }


        await context.RespondAsync(embed);
    }
}