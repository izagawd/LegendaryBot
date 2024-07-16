using System.ComponentModel;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.Entities;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using DiscordBotNet.LegendaryBot.StatusEffects;
using DSharpPlus.Commands;
using DSharpPlus.Entities;

namespace DiscordBotNet.LegendaryBot.Commands;

public class ViewDescription : GeneralCommandClass
{
    [Command("view-description"), Description("Lets you see details of a character, gear, blessing or status effect")]
    public async ValueTask Execute(CommandContext context, [Parameter("entity-name")] string entityName)
    {
        var simplifiedName = entityName.Replace(" ", "").ToLower();
        var zaObject = DefaultObjects.GetDefaultObjectsThatSubclass<Entity>()
            .FirstOrDefault(i => i.GetType().Name.ToLower() == simplifiedName);

        var zaColor = await DatabaseContext.UserData.FindOrCreateSelectUserDataAsync(context.User.Id, i => i.Color);
        var builder = new DiscordEmbedBuilder()
            .WithUser(context.User)
            .WithTitle("Entity description")
            .WithColor(zaColor);
        if (zaObject is not null)
        {
            var zaDescription = $"{zaObject.Name}.\nRarity: {zaObject.Rarity}";
            if (zaObject is Character characterr)
            {
                zaDescription += $" | Element: {characterr.Element}";
                
            }

            zaDescription += $"\n{zaObject.Description}";
            builder.WithDescription(zaDescription);
            if (zaObject is Character character)
            {
                builder.AddField($"Basic Attack :crossed_swords:", character.BasicAttack.GetDescription(character));
                if(character.Skill is not null)
                    builder.AddField($"Skill :magic_wand:", character.Skill.GetDescription(character));
                if (character.Ultimate is not null)
                    builder.AddField("Ultimate :zap:", character.Ultimate.GetDescription(character));
            }
        }
        else
        {
            
            builder.WithDescription($"Entity of name {entityName} not found");
            
        }

        await context.RespondAsync(builder);
    }
}