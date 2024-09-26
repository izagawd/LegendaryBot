using BasicFunctionality;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using Entities;
using Entities.LegendaryBot;
using Entities.LegendaryBot.BattleSimulatorStuff;
using Entities.LegendaryBot.Entities.BattleEntities.Characters;
using Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Commands;

public class FightCharacter : GeneralCommandClass
{
    [Command("fight-chosen-character")]
    [BotCommandCategory(BotCommandCategory.Battle)]
    public async ValueTask FightCommand(CommandContext context, string enemyName,
        int count = 1)
    {
        if (context.User.Id != DiscordBot.Izasid && context.User.Id != DiscordBot.Testersid)
        {
            await context.RespondAsync("Only izagawd can use this command");
            return;
        }
        var userData = await DatabaseContext.Set<UserData>().IncludeTeamWithAllEquipments()
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
            var newOne = (Character)Activator.CreateInstance(type)!;
            newOne.Level = userData.EquippedPlayerTeam!.Select(i => i.Level).Average().Round();
            created.Add(newOne);
        }


        var playerTeam = userData.EquippedPlayerTeam!;
        playerTeam.LoadTeamStats();
        await MakeOccupiedAsync(userData);
        var battleSim = new BattleSimulator(playerTeam.SetEveryoneToMaxHealth(),
            new NonPlayerTeam(created).LoadTeamStats().SetEveryoneToMaxHealth());
        var interaction = (context as SlashCommandContext)?.Interaction;
        if (interaction is not null) await context.RespondAsync("hol up");

        await battleSim.StartAsync(context.Channel);
    }
}