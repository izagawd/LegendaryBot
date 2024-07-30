using System.ComponentModel;
using DiscordBotNet.Extensions;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;

namespace DiscordBotNet.LegendaryBot.Commands;
[Command("team"), Description("idk")]
public class TeamCommand : GeneralCommandClass
{
    
    
    
    
    
    



    [Command("equip-team"), Description("Use this Commands to change teams"),
    BotCommandCategory( BotCommandCategory.Team)]
    public async ValueTask ExecuteEquip(CommandContext context,
        [Parameter("team-name")] string teamName)
    {
        var anon = await DatabaseContext.UserData
            .Include(i => i.EquippedPlayerTeam)
            .Where(i => i.Id == context.User.Id)
            .Select(i => new
                
                {  tier = i.Tier, team = i.PlayerTeams.FirstOrDefault(j => j.TeamName.ToLower()
                    .Replace(" ","")== teamName.ToLower()), userData = i })
            .FirstOrDefaultAsync();

        if (anon is null || anon.tier == Tier.Unranked)
        {
            await AskToDoBeginAsync(context);
            return;
        }
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

    [Command("remove-character")]
    [BotCommandCategory(BotCommandCategory.Team)]
    public async ValueTask ExecuteRemoveFromTeam(CommandContext context,
        [Parameter("character-num"), Description("Number of the character")] int characterNumber,
        [Parameter("team-name"),  Description("Name of team you want to remove character from")]
        string teamName)
    {
    

  
        var userData = await DatabaseContext.UserData
            .Include(i => i.PlayerTeams.Where(j => j.TeamName.ToLower()
                                                   == teamName.ToLower()))
            .Include(i => i.EquippedPlayerTeam)
            .Include(i => i.Characters.Where(j => 
                                                  j.Number == characterNumber))
            .FirstOrDefaultAsync(i => i.Id == context.User.Id);
        if (userData is null || userData.Tier == Tier.Unranked)
        {
            await AskToDoBeginAsync(context);
            return;
        }
        var gottenTeam =  userData.PlayerTeams.FirstOrDefault(i => i.TeamName.ToLower()
                                                                   == teamName.ToLower());
        

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
        var character = userData
            .Characters
            .FirstOrDefault(i => i.Number == characterNumber);
        if (character is null)
        {
            embed.WithDescription($"Character with number {characterNumber} could not be found");
            await context.RespondAsync(embed);
            return;
        }


        if (!gottenTeam.Contains(character))
        {
            embed.WithDescription($"Character {character} with number {characterNumber} is not in team {gottenTeam.TeamName}");
            return;
        }
        gottenTeam.Remove(character);
        
        await DatabaseContext.SaveChangesAsync();

        embed.WithTitle("Success!").WithDescription($"{character} with number {characterNumber} has been removed from team {gottenTeam.TeamName}!");
        await context.RespondAsync(embed);

    }

    [Command("rename-team"), BotCommandCategory(BotCommandCategory.Team)]

    
    public async ValueTask ExecuteRenameTeam(CommandContext context,
        [Parameter("team-name")]
        string teamName,
        [Parameter("new-name")]
        string newName)
    {
        var userData = await DatabaseContext.UserData
            .Include(i => i.PlayerTeams)
            .FirstOrDefaultAsync(i => i.Id == context.User.Id); 
        if (userData is null || userData.Tier == Tier.Unranked)
        {
            await AskToDoBeginAsync(context);
            return;
        }
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
    [Command("add-character"), Description("adds a character to a team!"), BotCommandCategory(BotCommandCategory.Team)]
    public async ValueTask ExecuteAddToTeam(CommandContext context, [Parameter("character-num"), Description("Id of character you want to add")] int characterNumber,
        [Parameter("team-name")] string teamName)
    {


        var userData = await DatabaseContext.UserData
            .Include(i => i.PlayerTeams.Where(j => j.TeamName.ToLower() == teamName.ToLower()))
            .Include(i => i.EquippedPlayerTeam)
            .Include(i => i.Characters.Where(j =>
                                                  j.Number == characterNumber))
            .FirstOrDefaultAsync(i => i.Id == context.User.Id);
        if (userData is null || userData.Tier == Tier.Unranked)
        {
            await AskToDoBeginAsync(context);
            return;
        }
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
        var character = userData
            .Characters
            .FirstOrDefault(i => i.Number == characterNumber);
        if (character is null)
        {
            embed.WithDescription($"Character with number {characterNumber} could not be found");
            await context.RespondAsync(embed);
            return;
        }

        gottenTeam.Add(character);
        await DatabaseContext.SaveChangesAsync();

        embed.WithTitle("Success!").WithDescription($"{character} has been added to team {gottenTeam.TeamName}!");
        await context.RespondAsync(embed);

    }
}