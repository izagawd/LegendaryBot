using System.ComponentModel;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DSharpPlus.Entities;
using DSharpPlus.Commands;
using Microsoft.EntityFrameworkCore;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.command;
[Command("team")]
public class TeamCommand : GeneralCommandClass
{
    
    
    
    
    
    



    [Command("equip_team"), Description("Use this command to change teams")]
    public async ValueTask ExecuteEquip(CommandContext context,
        [Parameter("team_name")] string teamName)
    {
        var anon = await DatabaseContext.UserData
            .Include(i => i.EquippedPlayerTeam)
            .FindOrCreateSelectUserDataAsync((long)context.User.Id,
                i => new { team = i.PlayerTeams.FirstOrDefault(j => j.TeamName.ToLower() == teamName.ToLower()), userData = i });

        var userData = anon.userData;
        var embed = new DiscordEmbedBuilder()
            .WithUser(context.User)
            .WithColor(userData.Color)
            .WithTitle("Hmm")
            .WithDescription("Team does not seem to exist");
        if (anon.team is null)
        {
            await context.RespondAsync(embed);
            return;
        }


        userData.EquippedPlayerTeam = anon.team;
        await DatabaseContext.SaveChangesAsync();
        embed.WithTitle("Success!")
            .WithDescription($"Team {anon.team.TeamName} is now equipped!");
        await context.RespondAsync(embed);

    }

    [Command("remove_character")]
    [AdditionalCommand("/remove_character player",BotCommandType.Battle)]
    public async ValueTask ExecuteRemoveFromTeam(CommandContext context,
        [Parameter("character_name")] string characterName,
        [Parameter("team_name"),  Description("Name of team you want to remove character from")]
        string teamName)
    {
    

        var simplifiedCharacterName = characterName.ToLower().Replace(" ", "");
        var userData = await DatabaseContext.UserData
            .Include(i => i.PlayerTeams.Where(j => j.TeamName.ToLower() == teamName.ToLower()))
            .Include(i => i.EquippedPlayerTeam)
            .Include(i => i.Inventory.Where(j => j is Character
                                                 && EF.Property<string>(j, "Discriminator").ToLower() == simplifiedCharacterName))
            .FindOrCreateUserDataAsync((long)context.User.Id);
        var gottenTeam =  userData.PlayerTeams.FirstOrDefault(i => i.TeamName.ToLower() == teamName.ToLower());
        

        var embed = new DiscordEmbedBuilder()
            .WithUser(context.User)
            .WithColor(userData.Color)
            .WithTitle("Hmm")
            .WithDescription($"Team with name {teamName} does not exist");
        if (gottenTeam is null)
        {
            await context.RespondAsync(embed);
            return;
        }

        if (gottenTeam.Count <= 1)
        {
            embed.WithDescription("There should be at least one character in a team");
            await context.RespondAsync(embed);
            return;
        }
        var character = userData.Inventory
            .OfType<Character>()
            .FirstOrDefault(i => i.GetType().Name.ToLower() == simplifiedCharacterName);
        if (character is null)
        {
            embed.WithDescription($"Character with name {characterName} could not be found");
            await context.RespondAsync(embed);
            return;
        }


        if (!gottenTeam.Contains(character))
        {
            embed.WithDescription($"Character {character} is not in team {gottenTeam.TeamName}");
            return;
        }

        gottenTeam.Remove(character);
        
        await DatabaseContext.SaveChangesAsync();

        embed.WithTitle("Success!").WithDescription($"{character} has been removed from team {gottenTeam.TeamName}!");
        await context.RespondAsync(embed);

    }

    [Command("rename_team")]

    
    public async ValueTask ExecuteRenameTeam(CommandContext context,
        [Parameter("team_name")]
        string teamName,
        [Parameter("new_name")]
        string newName)
    {
        var userData = await DatabaseContext.UserData
            .Include(i => i.PlayerTeams)
            .FindOrCreateUserDataAsync((long)context.User.Id);

        var embed = new DiscordEmbedBuilder()
            .WithColor(userData.Color)
            .WithUser(context.User)
            .WithTitle("Hmm")
            .WithDescription($"You do not have a team with name {teamName}");
        var team = userData.PlayerTeams.FirstOrDefault(i => i.TeamName.ToLower() == teamName);
        if (team is null)
        {
            await context.RespondAsync(embed);
            return;
        }

        if (userData.PlayerTeams.Except([team]).Any(i => i.TeamName.ToLower() == newName.ToLower()))
        {
            embed.WithDescription($"You already have a team with the name {newName}");
            await context.RespondAsync(embed);
            return;
        }

        team.TeamName = newName;
        await DatabaseContext.SaveChangesAsync();
        embed.WithTitle("Success!")
            .WithDescription($"Team {teamName} is now {newName!}");

        await context.RespondAsync(embed);

    }
    [Command("add_character"), Description("adds a character to a team!")]
    public async ValueTask ExecuteAddToTeam(CommandContext context, [Parameter("character_name"), Description("Name of team you want to add character to")] string characterName,
        [Parameter("team_name")] string teamName)
    {


        var simplifiedCharacterName = characterName.ToLower().Replace(" ", "");
        var userData = await DatabaseContext.UserData
            .Include(i => i.PlayerTeams.Where(j => j.TeamName.ToLower() == teamName.ToLower()))
            .Include(i => i.EquippedPlayerTeam)
            .Include(i => i.Inventory.Where(i => i is Character
                                                 && EF.Property<string>(i, "Discriminator").ToLower() == simplifiedCharacterName))
            .FindOrCreateUserDataAsync((long)context.User.Id);
        var gottenTeam = userData.PlayerTeams.FirstOrDefault(i => i.TeamName.ToLower() == teamName.ToLower());
        

        var embed = new DiscordEmbedBuilder()
            .WithUser(context.User)
            .WithColor(userData.Color)
            .WithTitle("Hmm")
            .WithDescription($"Team with name {teamName} does not exist");
        if (gottenTeam is null)
        {
            await context.RespondAsync(embed);
            return;
        }

        if (gottenTeam.IsFull)
        {
            embed.WithDescription("The provided team is full");
            await context.RespondAsync(embed);
            return;
        }
        var character = userData.Inventory
            .OfType<Character>()
            .FirstOrDefault(i => i.GetType().Name.ToLower() == simplifiedCharacterName);
        if (character is null)
        {
            embed.WithDescription($"Character with name {characterName} could not be found");
            await context.RespondAsync(embed);
            return;
        }

        gottenTeam.Add(character);
        await DatabaseContext.SaveChangesAsync();

        embed.WithTitle("Success!").WithDescription($"{character} has been added to team {gottenTeam.TeamName}!");
        await context.RespondAsync(embed);

    }
}