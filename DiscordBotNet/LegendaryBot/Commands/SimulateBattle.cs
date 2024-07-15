using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.BattleSimulatorStuff;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Results;
using DSharpPlus.Commands;

namespace DiscordBotNet.LegendaryBot.command;

public class SimulateBattle : GeneralCommandClass
{
    [Command("simulate_battle")]
    public async ValueTask Execute(CommandContext context,[Parameter("battle_count")] long battleCount = 1)
    {
        List<Task<BattleResult>> pendingBattleResults = [];
        foreach (var i in Enumerable.Range(0,(int)battleCount))
        {
            
            var battleSim = new BattleSimulator([new CoachChad(), new CoachChad(), new CoachChad(), new CoachChad()],
                [new CoachChad(), new CoachChad(), new CoachChad(), new CoachChad()]);
            foreach (var j in battleSim.CharacterTeams)
            {
                 j.LoadTeamEquipment();
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