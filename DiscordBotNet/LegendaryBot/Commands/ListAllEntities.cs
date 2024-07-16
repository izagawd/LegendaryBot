using System.Text;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.Entities;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Blessings;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears;
using DiscordBotNet.LegendaryBot.Entities.Items;
using DiscordBotNet.LegendaryBot.Entities.Items.ExpIncreaseMaterial;
using DSharpPlus.Commands;
using DSharpPlus.Entities;

namespace DiscordBotNet.LegendaryBot.Commands;

public class ListAllEntities : GeneralCommandClass
{
    private static readonly string listed;
    static ListAllEntities()
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

        listed = builder.ToString();

    }
    [Command("list-all-entities")]
    public async ValueTask ExecuteGetTotalMemoryUsedInBytes(CommandContext ctx)
    {
        var embed = new DiscordEmbedBuilder()
            .WithUser(ctx.User)
            .WithTitle("All possible entities")
            .WithDescription(listed)
            .WithColor(await DatabaseContext.UserData.FindOrCreateSelectUserDataAsync(ctx.User.Id, i => i.Color));
        await ctx.RespondAsync(new DiscordInteractionResponseBuilder().AsEphemeral()
            .AddEmbed(embed));
    }
}