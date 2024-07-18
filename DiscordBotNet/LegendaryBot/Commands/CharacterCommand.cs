using System.ComponentModel;
using System.Text;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.Entities;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Blessings;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;

namespace DiscordBotNet.LegendaryBot.Commands;

[Command("character")]
public class CharacterCommand : TeamCommand
{
    [Command("equip-blessing"), Description("Use this Command make a character equip a blessing")]
    public async ValueTask ExecuteEquipBlessing(CommandContext context,
        [Parameter("character-name")] string characterName,
        [Parameter("blessing-id")] Guid blessingId)
    {
        var simplifiedCharacterName = characterName.ToLower().Replace(" ", "");
        var userData = await DatabaseContext.UserData
            .Include(i => i.Blessings.Where(j => j.Id == blessingId))
            .Include(i => i.Characters.Where(j => 
                                              EF.Property<string>(j, "Discriminator").ToLower() ==
                                              simplifiedCharacterName))
            
                       
            .FirstOrDefaultAsync(i => i.Id == context.User.Id);
        if (userData is null || userData.Tier == Tier.Unranked)
        {
            await AskToDoBeginAsync(context);
            return;
        }
        var blessing = userData.Inventory.OfType<Blessing>().FirstOrDefault();
        var embed = new DiscordEmbedBuilder()
            .WithTitle("Equipping blessing")
            .WithUser(context.User)
            .WithColor(userData.Color);
        if (blessing is null)
        {
            embed.WithDescription("Blessing not found");
            await context.RespondAsync(embed);
            return;
        }

        var character = userData.Inventory.OfType<Character>().FirstOrDefault();
        if (character is null)
        {
            embed.WithDescription("Character not found");
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
        [Parameter("character-name")] string characterName,
        [Parameter("gear-id")] Guid gearId)
    {
        var simplifiedCharacterName = characterName.ToLower().Replace(" ", "");
        var userData = await DatabaseContext.UserData
            .Include(i => i.Gears)
            .Include(i => 
                i.Characters.Where(j => 
                EF.Property<string>(j, "Discriminator").ToLower() ==
                                                     simplifiedCharacterName))
            .ThenInclude(i => i.Gears)
            .Include(i => i.Gears.Where(j => j.Id == gearId))
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
        var character = userData.Inventory.OfType<Character>().FirstOrDefault();
        if (character is null)
        {
            embed.WithDescription("Character not found");
            await context.RespondAsync(embed);
            return;
        }
        var gear = userData.Inventory.OfType<Gear>().FirstOrDefault(i => i.Id == gearId);
        if (gear is null)
        {
            embed.WithDescription("Gear not found");
            await context.RespondAsync(embed);
            return;
        }

        character.Gears.Select(i => i.Name.Print()).ToArray();
        var stringBuilder =
            new StringBuilder(
                $"{character.Name} has successfully equipped {gear.Name} that has the following stats:\n");
        stringBuilder.Append($"Mainstat = {gear.MainStat.AsNameAndValue()}\n\n");
        if (gear.Substats.Any())
        {
            stringBuilder.Append("Substats:\n");
            foreach (var i in gear.Substats)
            {
                stringBuilder.Append(i.AsNameAndValue() + "\n");
            }
        }
     
        character.Gears.RemoveAll(i => i.GetType() == gear.GetType());
        character.Gears.Add(gear);
        await DatabaseContext.SaveChangesAsync();
        embed.WithDescription(stringBuilder.ToString());
        await context.RespondAsync(embed);

    }
}