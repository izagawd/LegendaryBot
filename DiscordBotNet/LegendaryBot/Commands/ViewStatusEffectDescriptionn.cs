using System.ComponentModel;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.Entities;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using DiscordBotNet.LegendaryBot.StatusEffects;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;

namespace DiscordBotNet.LegendaryBot.Commands;

public class ViewDescription : GeneralCommandClass
{
    [Command("view-description"), Description("Lets you see details of a character, gear, blessing or status effect")]
    public async ValueTask Execute(CommandContext context, [Parameter("entity-name")] string entityName)
    {
        var simplifiedName = entityName.Replace(" ", "").ToLower();
        object? zaObject = DefaultObjects.GetDefaultObjectsThatSubclass<Entity>()
            .FirstOrDefault(i => i.GetType().Name.ToLower() == simplifiedName);
        if (zaObject is null)
        {
            zaObject = DefaultObjects.GetDefaultObjectsThatSubclass<StatusEffect>()
                .FirstOrDefault(i => i.GetType().Name.ToLower() == simplifiedName);
        }

        var zaColor = await DatabaseContext.UserData
            .Where(i => i.Id == context.User.Id)
            .Select(i => i.Color)
            .FirstOrDefaultAsync();
        var builder = new DiscordEmbedBuilder()
            .WithUser(context.User)
            .WithTitle("Entity description")
            .WithColor(zaColor);
        if (zaObject is Entity zaEntity)
        {
            var zaDescription = $"{zaEntity.Name}.\nRarity: {zaEntity.Rarity}";
            if (zaEntity is Character z)
            {
                zaDescription += $" | Element: {z.Element}";
                
            }

            zaDescription += $"\n{zaEntity.Description}";
            builder.WithDescription(zaDescription);
            if (zaEntity is Character character)
            {
                builder.AddField($"Basic Attack :crossed_swords:", character.BasicAttack.GetDescription(character));
                if(character.Skill is not null)
                    builder.AddField($"Skill :magic_wand:", character.Skill.GetDescription(character));
                if (character.Ultimate is not null)
                    builder.AddField("Ultimate :zap:", character.Ultimate.GetDescription(character));
            }
        } else if (zaObject is StatusEffect statusEffect)
        {
            builder.WithDescription($"Name: {statusEffect.Name} | Type: {statusEffect.EffectType}\n{statusEffect.Description}");
        }
        else
        {
            
            builder.WithDescription($"Entity of name {entityName} not found");
            
        }

        await context.RespondAsync(builder);
    }
}