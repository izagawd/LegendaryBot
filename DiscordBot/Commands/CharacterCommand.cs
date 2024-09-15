using System.Collections.Immutable;
using System.ComponentModel;
using System.Text;
using BasicFunctionality;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Entities;
using Entities.LegendaryBot;
using Entities.LegendaryBot.Entities.BattleEntities.Gears;
using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Commands;

[Command("character")]
[Description("some commands related to character")]
public partial class CharacterCommand : GeneralCommandClass
{
    private static readonly ImmutableArray<string> _possibleGears = TypesFunction
        .GetDefaultObjectsAndSubclasses<Gear>()
        .Select(i => i.Name.ToLower().Replace(" ", "")).ToImmutableArray();


    [Command("equip-blessing")]
    [Description("Use this Command make a character equip a blessing")]
    [BotCommandCategory(BotCommandCategory.Character)]
    public async ValueTask ExecuteEquipBlessing(CommandContext context,
        [Parameter("character-num")] int characterNumber,
        [Parameter("blessing-name")] string blessingName)
    {
        var gotten = await DatabaseContext.Set<UserData>()
            .Include(i => i.Characters.Where(j =>
                j.Number == characterNumber))
            .ThenInclude(i => i.Blessing)
            .Include(i => i.Blessings)
            .FirstOrDefaultAsync(i => i.DiscordId == context.User.Id);
        if (gotten is null || gotten.Tier == Tier.Unranked)
        {
            await AskToDoBeginAsync(context);
            return;
        }


        if (gotten.IsOccupied)
        {
            await NotifyAboutOccupiedAsync(context);
            return;
        }

        var simplifiedName = blessingName.Replace(" ", "").ToLower();
        var possibleBlessings = gotten.Blessings
            .Where(i => i.Name.ToLower().Replace(" ", "") == simplifiedName)
            .ToArray();
        var embed = new DiscordEmbedBuilder()
            .WithTitle("Equipping blessing")
            .WithUser(context.User)
            .WithColor(gotten.Color);
        if (!possibleBlessings.Any())
        {
            embed.WithDescription($"Blessing with name {blessingName} not found in your inventory");
            await context.RespondAsync(embed);
            return;
        }

        var blessing = possibleBlessings.FirstOrDefault(i => i.CharacterId == null);
        if (blessing is null)
        {
            embed.WithDescription(
                $"Blessing with name {blessingName} found, but there isn't any that isn't already equipped by a character");
            await context.RespondAsync(embed);
            return;
        }


        var character = gotten.Characters.FirstOrDefault(i => i.Number == characterNumber);
        if (character is null)
        {
            embed.WithDescription($"Character with number {characterNumber} not found");
            await context.RespondAsync(embed);
            return;
        }

        character.Blessing = blessing;
        await DatabaseContext.SaveChangesAsync();
        var toSend = $"{character.Name} [{character.Number}] has successfully equipped {blessing.Name}!";
        embed.WithDescription(toSend);
        await context.RespondAsync(embed);
    }

    [Command("remove-blessing")]
    [Description("Use this Command make a character equip a blessing")]
    [BotCommandCategory(BotCommandCategory.Character)]
    public async ValueTask ExecuteRemoveBlessing(CommandContext context,
        [Parameter("character-num")] int characterNum)
    {
        var userData = await DatabaseContext.Set<UserData>()
            .Include(i => i.Characters.Where(j =>
                j.Number == characterNum))
            .ThenInclude(i => i.Blessing)
            .FirstOrDefaultAsync(i => i.DiscordId == context.User.Id);
        if (userData is null || userData.Tier == Tier.Unranked)
        {
            await AskToDoBeginAsync(context);
            return;
        }

        if (userData.IsOccupied)
        {
            await NotifyAboutOccupiedAsync(context);
            return;
        }


        var embed = new DiscordEmbedBuilder()
            .WithTitle("Removing blessing")
            .WithUser(context.User)
            .WithColor(userData.Color);

        var character = userData.Characters.FirstOrDefault();
        if (character is null)
        {
            embed.WithDescription($"Character with number {characterNum} not found");
            await context.RespondAsync(embed);
            return;
        }

        if (character.Blessing is null)
        {
            embed.WithDescription(
                $"Character {character.Name} [{character.Number}] does not have any blessing equipped");
            await context.RespondAsync(embed);
            return;
        }


        var prevBlessing = character.Blessing;
        character.Blessing = null;
        await DatabaseContext.SaveChangesAsync();
        embed.WithDescription(
            $"{character.Name} [{character.Number}] has successfully removed {prevBlessing.Name}!");
        await context.RespondAsync(embed);
    }


    [Command("equip-gear")]
    [Description("Use this Command make a character equip a gear")]
    [BotCommandCategory(BotCommandCategory.Character)]
    public async ValueTask ExecuteEquipGear(CommandContext context,
        [Parameter("character-num")] int characterNumber,
        [Parameter("gear-num")] int gearNumber)
    {
        var userData = await DatabaseContext.Set<UserData>()
            .Include(i => i.Gears)
            .Include(i =>
                i.Characters.Where(j =>
                    j.Number == characterNumber))
            .ThenInclude(i => i.Gears)
            .Include(i => i.Gears.Where(j => j.Number == gearNumber))
            .ThenInclude(i => i.Stats)
            .FirstOrDefaultAsync(i => i.DiscordId == context.User.Id);
        if (userData is null || userData.Tier == Tier.Unranked)
        {
            await AskToDoBeginAsync(context);
            return;
        }

        if (userData.IsOccupied)
        {
            await NotifyAboutOccupiedAsync(context);
            return;
        }

        var embed = new DiscordEmbedBuilder()
            .WithTitle("Equipping gear")
            .WithUser(context.User)
            .WithColor(userData.Color);
        var character = userData.Characters.FirstOrDefault();
        if (character is null)
        {
            embed.WithDescription($"Character with number {characterNumber} not found");
            await context.RespondAsync(embed);
            return;
        }

        var gear = userData.Gears.FirstOrDefault(i => i.Number == gearNumber);
        if (gear is null)
        {
            embed.WithDescription($"Gear with number {gearNumber} not not found");
            await context.RespondAsync(embed);
            return;
        }

        character.Gears.RemoveAll(i => i.GetType() == gear.GetType());
        character.Gears.Add(gear);
        var stringBuilder =
            new StringBuilder(
                $"{character.Name} [{character.Number}] has successfully equipped {gear.Name} that has the following stats:\n{gear.DisplayString}");


        await DatabaseContext.SaveChangesAsync();
        embed.WithDescription(stringBuilder.ToString());
        await context.RespondAsync(embed);
    }

    [Command("remove-gear")]
    [Description("Use this Command make a character remove an equipped gear")]
    [BotCommandCategory(BotCommandCategory.Character)]
    public async ValueTask ExecuteRemoveGear(CommandContext context,
        [Parameter("character-num")] int characterNumber,
        [Parameter("gear-type")] [SlashChoiceProvider<GearTypeProvider>]
        string gearType)
    {
        gearType = gearType.ToLower().Replace(" ", "");
        var userData = await DatabaseContext.Set<UserData>()
            .Include(i => i.Characters.Where(j => j.Number == characterNumber))
            .ThenInclude(i => i.Gears)
            .ThenInclude(i => i.Stats)
            .FirstOrDefaultAsync(i => i.DiscordId == context.User.Id);
        if (userData is null || userData.Tier == Tier.Unranked)
        {
            await AskToDoBeginAsync(context);
            return;
        }

        if (userData.IsOccupied)
        {
            await NotifyAboutOccupiedAsync(context);
            return;
        }

        var embed = new DiscordEmbedBuilder()
            .WithTitle("Removing gear")
            .WithUser(context.User)
            .WithColor(userData.Color);
        var character = userData.Characters.FirstOrDefault();
        if (character is null)
        {
            embed.WithDescription($"Character with number {characterNumber} not found");
            await context.RespondAsync(embed);
            return;
        }

        if (!_possibleGears.Contains(gearType))
        {
            embed.WithDescription($"Gear of type {gearType} doesnt exist");
            await context.RespondAsync(embed);
            return;
        }

        var gear = userData.Gears.FirstOrDefault(i => i.Name.ToLower().Replace(" ", "") == gearType);
        if (gear is null)
        {
            embed.WithDescription($"Your character does not have any **{gearType}** equipped");
            await context.RespondAsync(embed);
            return;
        }

        character.Gears.Remove(gear);
        var stringBuilder =
            new StringBuilder(
                $"{character.Name} [{character.Number}] has successfully removed {gear.Name} that has the following stats:\n{gear.DisplayString}");


        await DatabaseContext.SaveChangesAsync();
        embed.WithDescription(stringBuilder.ToString());
        await context.RespondAsync(embed);
    }

    private class GearTypeProvider : IChoiceProvider
    {
        private static readonly IReadOnlyDictionary<string, object> _daysOfTheWeek = new Dictionary<string, object>
        {
            { "Armor", "armor" },
            { "Weapon", "weapon" },
            { "Ring", "ring" },
            { "Necklace", "necklace" },
            { "Boots", "boots" },
            { "Helmet", "helmet" }
        }.AsReadOnly();

        public ValueTask<IReadOnlyDictionary<string, object>> ProvideAsync(CommandParameter parameter)
        {
            return ValueTask.FromResult(_daysOfTheWeek);
        }
    }
}