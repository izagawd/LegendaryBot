using System.ComponentModel;
using DiscordBotNet.Database.Models;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.Entities;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using DiscordBotNet.LegendaryBot.StatusEffects;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Functionality;
using Microsoft.EntityFrameworkCore;

namespace DiscordBotNet.LegendaryBot.Commands;

public class ViewDescription : GeneralCommandClass
{
    [Command("view-description"), Description("Lets you see details of a character, gear, blessing or status effect")]
    public async ValueTask Execute(CommandContext context, [Parameter("entity-name")] string entityName)
    {
        var simplifiedName = entityName.Replace(" ", "").ToLower();
        object? zaObject = TypesFunction.GetDefaultObjectsAndSubclasses<IInventoryEntity>()
            .FirstOrDefault(i => i.Name.ToLower().Replace(" ","") == simplifiedName);
        if (zaObject is null)
        {
            zaObject = TypesFunction.GetDefaultObjectsAndSubclasses<StatusEffect>()
                .FirstOrDefault(i => i.Name.ToLower().Replace(" ","") == simplifiedName);
        }

        var zaColor = await DatabaseContext.UserData
            .Where(i => i.DiscordId == context.User.Id)
            .Select(i => new DiscordColor?(i.Color))
            .FirstOrDefaultAsync();
        var builder = new DiscordEmbedBuilder()
            .WithUser(context.User)
            .WithTitle($"Description")
            .WithColor(zaColor
                .GetValueOrDefault(TypesFunction.GetDefaultObject<UserData>().Color));
        if (zaObject is IInventoryEntity zaEntity)
        {
            var zaDescription = $"{zaEntity.TypeGroup.Name}: {zaEntity.Name}.\nRarity: {(int) zaEntity.Rarity} :star:";
            if (zaEntity is Character z)
            {
                zaDescription += $" • Element: {z.Element}";
                
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
                if (character.PassiveDescription is not null)
                {
                    builder.AddField("Passive", character.PassiveDescription);
                }
                var statsToIncreasePerMilestone = character.GetStatsToIncreaseBasedOnLevelMilestone(6).ToArray();
                var stringToUse = "";
                for (var i = 0; i < statsToIncreasePerMilestone.Length; i++)
                {
                    stringToUse +=
                        $"After reaching level {(i + 1) * 10}, {statsToIncreasePerMilestone[i].GetShortName()} increases by {Character.GetStatIncreaseMilestoneValueString(statsToIncreasePerMilestone[i])} additionally!\n";
                }

                stringToUse +=
                    $"\nAttack increases by {Character.MilestoneFlatAttackIncrease} and health increases by {Character.MilestoneFlatHealthIncrease} additionally for every 10 levels as well.";
                builder.AddField("Level milestone stat increase", stringToUse);
            }
        } else if (zaObject is StatusEffect statusEffect)
        {
            builder.WithDescription($"Status Effect: {statusEffect.Name} • Type: {statusEffect.EffectType}\n{statusEffect.Description}");
        }
        else
        {
            
            builder.WithDescription($"InventoryEntity of name {entityName} not found");
            
        }

        await context.RespondAsync(builder);
    }
}