
using DiscordBotNet.Database.Models;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.BattleSimulatorStuff;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;

namespace DiscordBotNet.LegendaryBot.command;

public class Challenge :GeneralCommandClass
{   

    private static readonly DiscordButtonComponent yes = new(ButtonStyle.Primary, "yes", "YES");
    private static readonly DiscordButtonComponent no = new(ButtonStyle.Primary, "no", "NO");

    [SlashCommand("challenge", "Challenge other players to a duel!"),
    AdditionalSlashCommand("/challenge @user",BotCommandType.Battle)]
    public async Task Execute(InteractionContext ctx,[Option("user", "User to challenge")] DiscordUser opponent)
    {
        
        var player1 = ctx.User;
        var player2 = opponent;

        var player1User = await DatabaseContext.UserData
            .IncludeTeamWithAllEquipments()
            .FindOrCreateUserDataAsync((long)player1.Id);
        DiscordEmbedBuilder embedToBuild;
        if (player1User.IsOccupied)
        {
            embedToBuild = new DiscordEmbedBuilder()
                .WithTitle($"Hmm")
                .WithColor(player1User.Color)
                .WithAuthor(player1.Username, iconUrl: player1.AvatarUrl)
                .WithDescription("You are occupied");
            await ctx.CreateResponseAsync(embedToBuild.Build());
            return;
        }
        embedToBuild = new DiscordEmbedBuilder()
            .WithTitle($"Hmm")
            .WithColor(player1User.Color)
            .WithAuthor(player1.Username, iconUrl: player1.AvatarUrl)
            .WithDescription("You cannot fight yourself");

        if (player1.Id == player2.Id)
        {
            await ctx.CreateResponseAsync(embedToBuild.Build());
            return;
            
        }

        var player2User = await DatabaseContext.UserData
            .IncludeTeamWithAllEquipments()
            .FindOrCreateUserDataAsync((long)player2.Id);
        if (player2User.IsOccupied)
        {
            embedToBuild = new DiscordEmbedBuilder()
                .WithTitle($"Hmm")
                .WithColor(player1User.Color)
                .WithAuthor(player1.Username, iconUrl: player1.AvatarUrl)
                .WithDescription($"{player2.Username} is occupied");
            await ctx.CreateResponseAsync(embedToBuild.Build());
            return;
        }
        if (player2User.Tier == Tier.Unranked || player1User.Tier == Tier.Unranked)
        {
            embedToBuild = embedToBuild
                .WithTitle($"Hmm")
                .WithDescription("One of you have not begun your journey with /begin");
            await ctx.CreateResponseAsync(embedToBuild.Build());
            return;
            
        }
        embedToBuild = embedToBuild
            .WithTitle($"{player2.Username}, ")
            
            .WithDescription($"`do you accept {player1.Username}'s challenge?`");
        await MakeOccupiedAsync(player1User);
        var response = new DiscordInteractionResponseBuilder()
            .AddEmbed(embedToBuild.Build())
            .AddComponents(yes, no);
        await ctx.CreateResponseAsync(response);
        var message = await ctx.GetOriginalResponseAsync();
        string? decision = null;
        var interactivityResult = await message.WaitForButtonAsync(player2);
        if (!interactivityResult.TimedOut)
        {
            decision = interactivityResult.Result.Id;
        }
        else
        {
            await message.ModifyAsync(new DiscordMessageBuilder()
                .WithEmbed(new DiscordEmbedBuilder()
                    .WithTitle("Hmm")
                    .WithUser(player1)
                    .WithDescription("Time out")
                    .WithColor(player1User.Color)));
            return;
        }

        if (decision == "no")
        {
            var responseBuilder = new DiscordInteractionResponseBuilder()
                .AddEmbed(embedToBuild.WithTitle($"Hmm")

                    .WithDescription("Battle request declined")
                    .WithColor(player1User.Color)
                    .WithUser(player1)
                    .Build());
            await interactivityResult.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
                responseBuilder);
            return;
        }

        await interactivityResult.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
        await MakeOccupiedAsync(player2User);
        var player1Team = player1User.EquippedPlayerTeam!.LoadTeamEquipment();
        var player2Team = player2User.EquippedPlayerTeam!.LoadTeamEquipment();
        var simulator = new BattleSimulator(player1Team,player2Team);
        var battleResult = await simulator.StartAsync(message);
        DiscordUser winnerDiscord;
        UserData winnerUserData;
        if (battleResult.Winners.TryGetUserDataId.GetValueOrDefault(0) == (long)player1.Id)
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
        {
            Embed = new DiscordEmbedBuilder()
                .WithColor(winnerUserData.Color)
                .WithTitle("Battle Ended")
                .WithDescription($"{winnerDiscord.Username} won the battle! ")
        });
    }
}