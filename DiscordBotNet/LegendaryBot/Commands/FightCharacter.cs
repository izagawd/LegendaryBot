using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.BattleSimulatorStuff;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;

namespace DiscordBotNet.LegendaryBot.Commands;

public class FightCharacter : GeneralCommandClass
{
    [Command("fight-chosen-character")]
    public async ValueTask FightCommand(CommandContext context, string enemyName)
    {
        if (context.User.Id != Bot.Izasid)
        {
            await context.RespondAsync("Only izagawd can use this command");
        }
        var userData =await  DatabaseContext.UserData.IncludeTeamWithAllEquipments()
            .FirstOrDefaultAsync(i => i.Id == context.User.Id);
        if (userData is null || userData.Tier == Tier.Unranked)
        {
            await AskToDoBeginAsync(context);
            return;
        }

        var type =TypesFunctionality.GetDefaultObjectsThatIsInstanceOf<Character>()
            .Where(i => i.GetType().Name.ToLower() == enemyName.ToLower().Replace(" ", ""))
            .Select(i => i.GetType())
            .FirstOrDefault();
        if (type is null)
        {
            await context.RespondAsync("character not found");
            return;
        }

        var createdChar = (Character) Activator.CreateInstance(type)!;
        createdChar.Level = userData.EquippedPlayerTeam.Select(i => i.Level).Average().Round();
        await MakeOccupiedAsync(userData);
        var battleSim = new BattleSimulator(userData.EquippedPlayerTeam.LoadTeamStats(),
            new CharacterTeam(createdChar).LoadTeamStats());
        var interaction = (context as SlashCommandContext)?.Interaction;
        if (interaction is not null)
        {
            await interaction.CreateResponseAsync(DiscordInteractionResponseType.DeferredChannelMessageWithSource);
        }

        await battleSim.StartAsync(context.Channel);
        
     
    }
}