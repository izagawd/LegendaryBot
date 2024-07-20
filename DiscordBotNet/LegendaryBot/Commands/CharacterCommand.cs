using System.ComponentModel;
using System.Text;
using DiscordBotNet.Extensions;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;

namespace DiscordBotNet.LegendaryBot.Commands;

[Command("character")]
public class CharacterCommand : GeneralCommandClass
{
    [Command("equip-blessing"), Description("Use this Command make a character equip a blessing")]
    public async ValueTask ExecuteEquipBlessing(CommandContext context,
        [Parameter("character-number")] int characterNumber,
        [Parameter("blessing-name")] string blessingName)
    {

        var userData = await DatabaseContext.UserData
            .Include(i => i.Blessings)
            .ThenInclude(i => i.Character)
            .Include(i => i.Characters.Where(j => 
                                              j.Number == characterNumber))
            .FirstOrDefaultAsync(i => i.Id == context.User.Id);
        if (userData is null || userData.Tier == Tier.Unranked)
        {
            await AskToDoBeginAsync(context);
            return;
        }

        var simplifiedName = blessingName.Replace(" ", "").ToLower();
        var possibleBlessings = userData.Blessings
            .Where(i => i.Name.ToLower().Replace(" ","") == simplifiedName)
            .ToArray();
        var embed = new DiscordEmbedBuilder()
            .WithTitle("Equipping blessing")
            .WithUser(context.User)
            .WithColor(userData.Color);
        if (!possibleBlessings.Any())
        {
            embed.WithDescription($"Blessing with name {blessingName} not found in your inventory");
            await context.RespondAsync(embed);
            return;
        }

        var blessing = possibleBlessings.FirstOrDefault(i => i.Character == null);
        if (blessing is null)
        {
            embed.WithDescription($"Blessing with name {blessingName} found, but there isn't any that isn't already equipped by a character");
            await context.RespondAsync(embed);
            return;
        }

        
        var character = userData.Characters.FirstOrDefault();
        if (character is null)
        {
            embed.WithDescription($"Character with number {characterNumber} not found");
            await context.RespondAsync(embed);
            return;
        }
        
        character.Blessing = blessing;
        await DatabaseContext.SaveChangesAsync();
        embed.WithDescription(
            $"{character.Name} has successfully equipped {blessing.Name}!");
        await context.RespondAsync(embed);

    }
     
    
    [Command("equip-gear"), Description("Use this Command make a character equip a gear")]
    public async ValueTask ExecuteEquipGear(CommandContext context,
        [Parameter("character-number")] int characterNumber,
        [Parameter("gear-number")] int gearNumber)
    {

        var userData = await DatabaseContext.UserData
            .Include(i => i.Gears)
            .Include(i => 
                i.Characters.Where(j => 
                j.Number == characterNumber))
            .ThenInclude(i => i.Gears)
            .Include(i => i.Gears.Where(j => j.Number == gearNumber))
            .ThenInclude(i =>  i.Stats)
            .FirstOrDefaultAsync(i => i.Id == context.User.Id);
        if (userData is null || userData.Tier == Tier.Unranked)
        {
            await AskToDoBeginAsync(context);
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

        var stringBuilder =
            new StringBuilder(
                $"{character.Name} has successfully equipped {gear.Name} that has the following stats:\n{gear.DisplayString}");
     
     
        character.Gears.RemoveAll(i => i.GetType() == gear.GetType());
        character.Gears.Add(gear);
        await DatabaseContext.SaveChangesAsync();
        embed.WithDescription(stringBuilder.ToString());
        await context.RespondAsync(embed);

    }
}