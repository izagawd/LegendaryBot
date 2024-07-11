using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.BattleSimulatorStuff;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Results;
using DSharpPlus.SlashCommands;

namespace DiscordBotNet.LegendaryBot.command;

public class SimulateBattle : GeneralCommandClass
{
    [SlashCommand("simulate_battle","simulates a set number of battles")]
    public async Task Execute(InteractionContext context,[Option("battle_count","the battle count")] long battleCount = 1)
    {
        List<Task<BattleResult>> pendingBattleResults = [];
        foreach (var i in Enumerable.Range(0,(int)battleCount))
        {
            
            var battleSim = new BattleSimulator([new CoachChad(), new CoachChad(), new CoachChad(), new CoachChad()],
                [new CoachChad(), new CoachChad(), new CoachChad(), new CoachChad()]);
            foreach (var j in battleSim.CharacterTeams)
            {
                await j.LoadTeamGearWithPlayerDataAsync(context.User);
            }

            Task.Run(async () =>
            {
                try
                {
                    await battleSim.StartAsync(context.Channel);
                }
                catch (Exception e)
                {
                    e.Print();
                }
                
            });
            
        }
        
    }
    
}