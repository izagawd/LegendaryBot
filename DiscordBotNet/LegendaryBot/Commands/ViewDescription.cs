using System.ComponentModel;
using DiscordBotNet.Database.Models;
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
        object? zaObject = TypesFunctionality.GetDefaultObjectsThatIsInstanceOf<IInventoryEntity>()
            .FirstOrDefault(i => i.GetType().Name.ToLower() == simplifiedName);
        if (zaObject is null)
        {
            zaObject = TypesFunctionality.GetDefaultObjectsThatIsInstanceOf<StatusEffect>()
                .FirstOrDefault(i => i.GetType().Name.ToLower() == simplifiedName);
        }

        var zaColor = await DatabaseContext.UserData
            .Where(i => i.Id == context.User.Id)
            .Select(i => new DiscordColor?(i.Color))
            .FirstOrDefaultAsync();
        var builder = new DiscordEmbedBuilder()
            .WithUser(context.User)
            .WithTitle($"Description")
            .WithColor(zaColor
                .GetValueOrDefault(TypesFunctionality.GetDefaultObject<UserData>().Color));
        if (zaObject is IInventoryEntity zaEntity)
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
            
            builder.WithDescription($"InventoryEntity of name {entityName} not found");
            
        }

        await context.RespondAsync(builder);
    }
}