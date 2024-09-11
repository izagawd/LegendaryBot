using System.ComponentModel;
using BasicFunctionality;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using Entities;
using Entities.LegendaryBot;
using Entities.LegendaryBot.BattleSimulatorStuff;
using Entities.LegendaryBot.Entities.BattleEntities.Characters;
using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Commands;

public class Challenge : GeneralCommandClass
{
    private static readonly DiscordButtonComponent Yes = new(DiscordButtonStyle.Primary, "yes", "YES");
    private static readonly DiscordButtonComponent No = new(DiscordButtonStyle.Primary, "no", "NO");

    [Command("challenge")]
    [Description("Use this Commands to fight other players")]
    [BotCommandCategory(BotCommandCategory.Battle)]
    public async ValueTask Execute(CommandContext ctx,
        [Description("The player you want to fight")]
        DiscordUser opponent)
    {
        var player1 = ctx.User;
        var player2 = opponent;

        var player1User = await DatabaseContext.Set<UserData>()
            .IncludeTeamWithAllEquipments()
            .FirstOrDefaultAsync(i => i.DiscordId == player1.Id);
        if (player1User is null || player1User.Tier == Tier.Unranked)
        {
            await AskToDoBeginAsync(ctx);
            return;
        }

        DiscordEmbedBuilder embedToBuild;
        if (player1User.IsOccupied)
        {
            await NotifyAboutOccupiedAsync(ctx);
            return;
        }

        embedToBuild = new DiscordEmbedBuilder()
            .WithTitle("Hmm")
            .WithColor(player1User.Color)
            .WithAuthor(player1.Username, iconUrl: player1.AvatarUrl)
            .WithDescription("You cannot fight yourself");

        if (player1.Id == player2.Id)
        {
            await ctx.RespondAsync(embedToBuild.Build());
            return;
        }

        var player2User = await DatabaseContext.Set<UserData>()
            .IncludeTeamWithAllEquipments()
            .FirstOrDefaultAsync(i => i.DiscordId == player2.Id);

        if (player2User is null || player2User.IsOccupied || player2User.Tier == Tier.Unranked)
        {
            embedToBuild = new DiscordEmbedBuilder()
                .WithTitle("Hmm")
                .WithColor(player1User.Color)
                .WithAuthor(player1.Username, iconUrl: player1.AvatarUrl)
                .WithDescription($"{player2.Username} is occupied, or has not begun with /begin");
            await ctx.RespondAsync(embedToBuild.Build());
            return;
        }

        await DatabaseContext.Set<UserData>()
            .Where(i => i.DiscordId == player2.Id)
            .IncludeTeamWithAllEquipments()
            .LoadAsync();
        if (player2User.Tier == Tier.Unranked || player1User.Tier == Tier.Unranked)
        {
            embedToBuild = embedToBuild
                .WithTitle("Hmm")
                .WithDescription("One of you have not begun your journey with /begin");
            await ctx.RespondAsync(embedToBuild.Build());
            return;
        }

        embedToBuild = embedToBuild
            .WithTitle($"{player2.Username}, ")
            .WithDescription($"`do you accept {player1.Username}'s challenge?`");
        await MakeOccupiedAsync(player1User);
        var response = new DiscordInteractionResponseBuilder()
            .AddEmbed(embedToBuild.Build())
            .AddComponents(Yes, No);
        await ctx.RespondAsync(response);
        var message = (await ctx.GetResponseAsync())!;

        string? decision;
        var interactivityResult = await message.WaitForButtonAsync(player2);
        if (!interactivityResult.TimedOut)
        {
            decision = interactivityResult.Result.Id;
        }
        else
        {
            await message.ModifyAsync(new DiscordMessageBuilder()
                .AddEmbed(new DiscordEmbedBuilder()
                    .WithTitle("Hmm")
                    .WithUser(player1)
                    .WithDescription("Time out")
                    .WithColor(player1User.Color)));
            return;
        }

        if (decision == "no")
        {
            var responseBuilder = new DiscordInteractionResponseBuilder()
                .AddEmbed(embedToBuild.WithTitle("Hmm")
                    .WithDescription("Battle request declined")
                    .WithColor(player1User.Color)
                    .WithUser(player1)
                    .Build());
            await interactivityResult.Result.Interaction.CreateResponseAsync(
                DiscordInteractionResponseType.UpdateMessage,
                responseBuilder);
            return;
        }

        await interactivityResult.Result.Interaction.CreateResponseAsync(DiscordInteractionResponseType
            .DeferredMessageUpdate);
        await MakeOccupiedAsync(player2User);
        var player1Team = player1User.EquippedPlayerTeam!.LoadTeamStats();
        var player2Team = player2User.EquippedPlayerTeam!.LoadTeamStats();
        var simulator = new BattleSimulator(player1Team, player2Team);
        var battleResult = await simulator.StartAsync(message);
        DiscordUser winnerDiscord;
        UserData winnerUserData;
        if (((PlayerTeam)battleResult.Winners).UserData.DiscordId == player1.Id)
        {
            winnerDiscord = player1;
            winnerUserData = player1User;
        }
        else
        {
            winnerDiscord = player2;
            winnerUserData = player2User;
        }


        await DatabaseContext.SaveChangesAsync();
        await message.ModifyAsync(new DiscordMessageBuilder()
            .AddEmbed(new DiscordEmbedBuilder()
                .WithColor(winnerUserData.Color)
                .WithTitle("Battle Ended")
                .WithDescription($"{winnerDiscord.Username} won the battle! ").Build()));
    }
}