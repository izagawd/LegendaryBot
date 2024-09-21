using System.ComponentModel;
using BasicFunctionality;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Entities.LegendaryBot;
using Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Commands;

[Command("team")]
[Description("idk")]
public class TeamCommand : GeneralCommandClass
{
    [Command("equip-team")]
    [Description("Use this Commands to change teams")]
    [BotCommandCategory(BotCommandCategory.Team)]
    public async ValueTask ExecuteEquip(CommandContext context,
        [Parameter("team-name")] string teamName)
    {
        var userData = await DatabaseContext.Set<UserData>()
            .Include(i => i.PlayerTeams)
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

        var team = userData.PlayerTeams.FirstOrDefault(i => i.TeamName.Replace(" ", "")
            .Equals(teamName.Replace(" ", ""), StringComparison.OrdinalIgnoreCase));
        var embed = new DiscordEmbedBuilder()
            .WithUser(context.User)
            .WithColor(userData.Color)
            .WithTitle("Hmm")
            .WithDescription("Team does not seem to exist");
        if (team is null)
        {
            await context.RespondAsync(embed);
            return;
        }


        userData.EquippedPlayerTeam = team;
        await DatabaseContext.SaveChangesAsync();
        embed.WithTitle("Success!")
            .WithDescription($"Team {team.TeamName} is now equipped!");
        await context.RespondAsync(embed);
    }

    [Command("set-slot")] [Description("Puts a character into a team slot")]
    [BotCommandCategory(BotCommandCategory.Team)]
    public async ValueTask ExecuteChangeTeamCharacter(CommandContext context,
        [Parameter("team-name")] [Description("Name of team you want to remove character from")]
        string teamName, [Parameter("team-slot")] int teamSlot,
        [Parameter("character-name")] [Description("Name of the character. ignore if you are removng from slot")]
        string? characterName = null)
    {
        int typeIdToLookFor = Character.LookFor(characterName);
       
        var userData = await DatabaseContext.Set<UserData>()
            .Include(i => i.PlayerTeams.Where(j => j.TeamName.ToLower()
                                                   == teamName.ToLower()))
            .ThenInclude(i => i.TeamMemberships)
            .ThenInclude(i => i.Character)
            .Include(i => i.EquippedPlayerTeam)
            .ThenInclude(i => i!.TeamMemberships)
            .ThenInclude(i => i.Character)
            
            .Include(i => i.Characters.Where(j =>
                typeIdToLookFor == j.TypeId))
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

        var gottenTeam = userData.PlayerTeams.FirstOrDefault(i => i.TeamName.ToLower()
                                                                  == teamName.ToLower());


        var embed = new DiscordEmbedBuilder()
            .WithUser(context.User)
            .WithColor(userData.Color)
            .WithTitle("Hmm")
            .WithDescription($"Team with name {teamName} does not exist");
        if (teamSlot <= 0 || teamSlot > 4)
        {
            embed.WithDescription($"slot ranges from 1 to 4. {teamSlot} is an invalid number");
            await context.RespondAsync(embed);
            return;
        }

        if (gottenTeam is null)
        {
            await context.RespondAsync(embed);
            return;
        }

        if (gottenTeam.Count <= 1 && gottenTeam[teamSlot] is not null)
        {
            embed.WithDescription(
                $"There should be at least one character in a team, so you cant de equip from slot {teamSlot}");
            await context.RespondAsync(embed);
            return;
        }

        var character = userData
            .Characters
            .FirstOrDefault(i => i.TypeId == typeIdToLookFor);

        if (character is null && characterName is not null)
        {
            embed.WithDescription($"character with name {characterName} not found");
            await context.RespondAsync(embed);
            return;
        }

        var prevChar = gottenTeam[teamSlot];
        gottenTeam[teamSlot] = character;
        await DatabaseContext.SaveChangesAsync();

        string text;
        if (character is null && prevChar is not null)
            text = $"{prevChar.Name} has been removed from team {gottenTeam.TeamName}!";
        else if (character is null)
            text = $"No character in slot {teamSlot} of team {gottenTeam.TeamName}";
        else
            text = $"{character} has been put in slot {teamSlot} in team {gottenTeam.TeamName}";
        embed.WithTitle("Success!")
            .WithDescription(text);
        await context.RespondAsync(embed);
    }

    [Command("rename-team")] [Description("Renames a team")]
    [BotCommandCategory(BotCommandCategory.Team)]
    public async ValueTask ExecuteRenameTeam(CommandContext context,
        [Parameter("team-name")] string teamName,
        [Parameter("new-name")] string newName)
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

        if (userData.IsOccupied)
        {
            await NotifyAboutOccupiedAsync(context);
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
            .WithDescription($"Team {teamName} is now {newName}");

        await context.RespondAsync(embed);
    }
}