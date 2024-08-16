using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.BattleSimulatorStuff;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using Microsoft.EntityFrameworkCore;

namespace DiscordBotNet.LegendaryBot.Commands;

public class FightCharacter : GeneralCommandClass
{
    [Command("fight-chosen-character")]
    [BotCommandCategory(BotCommandCategory.Battle)]
    public async ValueTask FightCommand(CommandContext context, string enemyName,
        int count = 1)
    {
        if (context.User.Id != Bot.Izasid) await context.RespondAsync("Only izagawd can use this command");
        var userData = await DatabaseContext.UserData.IncludeTeamWithAllEquipments()
            .FirstOrDefaultAsync(i => i.DiscordId == context.User.Id);
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

        var type = TypesFunction.GetDefaultObjectsAndSubclasses<Character>()
            .Where(i => i.Name.ToLower().Replace(" ", "") == enemyName.ToLower().Replace(" ", ""))
            .Select(i => i.GetType())
            .FirstOrDefault();
        if (type is null)
        {
            await context.RespondAsync("character not found");
            return;
        }

        List<Character> created = [];
        foreach (var _ in Enumerable.Range(0, count))
        {
            var newOne = (Character)Activator.CreateInstance(type);
            newOne.Level = userData.EquippedPlayerTeam.Select(i => i.Level).Average().Round();
            created.Add(newOne);
        }


        await MakeOccupiedAsync(userData);
        var battleSim = new BattleSimulator(userData.EquippedPlayerTeam.LoadTeamStats(),
            new CharacterTeam(created).LoadTeamStats());
        var interaction = (context as SlashCommandContext)?.Interaction;
        if (interaction is not null) await context.RespondAsync("hol up");

        await battleSim.StartAsync(context.Channel);
    }
}