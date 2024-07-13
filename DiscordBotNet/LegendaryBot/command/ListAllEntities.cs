using System.Text;
using DiscordBotNet.LegendaryBot.Entities;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Blessings;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears;
using DiscordBotNet.LegendaryBot.Entities.Items;
using DiscordBotNet.LegendaryBot.Entities.Items.ExpIncreaseMaterial;
using DSharpPlus.Commands;

namespace DiscordBotNet.LegendaryBot.command;

public class ListAllEntities : GeneralCommandClass
{
    [Command("list_all_entities")]
    public async ValueTask ExecuteGetTotalMemoryUsedInBytes(CommandContext context)
    {
        var builder = new StringBuilder();
        foreach (var i in DefaultObjects.GetDefaultObjectsThatSubclass<Entity>()
                     .Select(i =>
                     {
                         Type type = null;
                         if (i is Character)
                             type = typeof(Character);
                         else if (i is Gear)
                             type = typeof(Gear);
                         else if (i is CharacterExpMaterial)
                             type = typeof(CharacterExpMaterial);
                         else if (i is Item)
                             type = typeof(Item);
                         else if (i is Blessing)
                             type = typeof(Blessing);
                         else
                             type = typeof(Entity);
                         return new { type, entity = i };
                     }).OrderBy(i => i.type?.Name))
        {
            builder.Append($"{i.entity.Name} ({i.type.Name})\n");
        }

        await context.RespondAsync(builder.ToString());
    }
}