using System.ComponentModel;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.command;
using DiscordBotNet.LegendaryBot.StatusEffects;
using DSharpPlus.Commands;
using DSharpPlus.Entities;

namespace DiscordBotNet.LegendaryBot.Commands;

public class ViewStatusEffectDescription : GeneralCommandClass
{
    [Command("view-status-effect-description"), Description("Lets you see what a status effect can do")]
    public async ValueTask Execute(CommandContext context, [Parameter("status-effect-name")] string statusEffectName)
    {
        var simplifiedName = statusEffectName.Replace(" ", "").ToLower();
        var zaObject = DefaultObjects.GetDefaultObjectsThatSubclass<StatusEffect>()
            .FirstOrDefault(i => i.GetType().Name.ToLower() == simplifiedName);

        var zaColor = await DatabaseContext.UserData.FindOrCreateSelectUserDataAsync(context.User.Id, i => i.Color);
        var builder = new DiscordEmbedBuilder()
            .WithUser(context.User)
            .WithTitle("Status effect description")
            .WithColor(zaColor);
        if (zaObject is not null)
        {
            builder.WithDescription($"{zaObject.Name}: {zaObject.Description}");
        }
        else
        {
            builder.WithDescription($"Status effect of name {statusEffectName} not found");
            
        }

        await context.RespondAsync(builder);
    }
}