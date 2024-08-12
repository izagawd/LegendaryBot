using System.Collections.Immutable;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.Entities.Items;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.EntityFrameworkCore;

namespace DiscordBotNet.LegendaryBot.Commands;

public class FarmGear : GeneralCommandClass
{
    private readonly ImmutableArray<Tier> _tiers = [Tier.Gold, Tier.Platinum, Tier.Diamond, Tier.Divine];
    [Command("farm-gear")]
    public async ValueTask ExecuteAsync(CommandContext context)
    {
        var userData = await DatabaseContext.UserData
            .IncludeTeamWithAllEquipments()
            .Include(i => i.Items.Where(j => j is Stamina))
            .FirstOrDefaultAsync(i => i.Id == context.User.Id);
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

        if (userData.Tier < Tier.Gold)
        {
            ;
            await context.RespondAsync(new DiscordEmbedBuilder()
                .WithTitle("Hmm")
                .WithUser(context.User)
                .WithColor(userData.Color)
                .WithDescription($"You need to be at least **{Tier.Gold}** tier before you can start farming gear")
                .Build());
            return;
        }

        var stamina = userData.Items.GetOrCreateItem<Stamina>();
        const int requiredStamina = 40;
        stamina.RefreshEnergyValue();
        if (stamina.Stacks < requiredStamina)
        {
            await context.RespondAsync(new DiscordEmbedBuilder()
                .WithTitle("Hmm")
                .WithUser(context.User)
                .WithColor(userData.Color)
                .WithDescription($"You need at least {requiredStamina} stamina to farm gear")
                .Build());
            return;
        }
        await MakeOccupiedAsync(userData);
        List<DiscordButtonComponent> buttonComponents = new List<DiscordButtonComponent>(_tiers.Length);

        foreach (var i in _tiers)
        {
            var button = new DiscordButtonComponent(DiscordButtonStyle.Success, ((byte)i).ToString(),
                i.ToString());
            if (userData.Tier < i)
                button.Disable();
            buttonComponents.Add(button);
        }

        var embed = new DiscordEmbedBuilder()
            .WithUser(context.User)
            .WithColor(userData.Color)
            .WithTitle($"{userData.Name},")
            .WithDescription("Select the tier of gear farming you want to indulge in");
        var messageBuilder = new DiscordMessageBuilder()
            .AddEmbed(embed)
            .AddComponents(buttonComponents);
        await context.RespondAsync(messageBuilder);
        var message = (await context.GetResponseAsync())!;
        var result = await message.WaitForButtonAsync(context.User);
        if (result.TimedOut)
        {
            foreach (var i in buttonComponents)
            {
                i.Disable();
            }
            await message.ModifyAsync(messageBuilder);
            return;
        }

        await result.Result.Interaction.CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage,
            new DiscordInteractionResponseBuilder().WithContent("gay"));
    


    }
}   