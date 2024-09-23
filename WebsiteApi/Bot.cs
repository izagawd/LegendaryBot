using System.Diagnostics;
using BasicFunctionality;
using DatabaseManagement;
using Entities.LegendaryBot;
using Entities.LegendaryBot.Entities;
using Entities.LegendaryBot.Entities.BattleEntities.Blessings;
using Entities.LegendaryBot.Entities.BattleEntities.Characters;
using Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using Entities.LegendaryBot.Entities.BattleEntities.Gears;
using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace WebsiteApi;

public static class Bot
{
    private static Task FirstTimeSetupAsync()
    {
        return new PostgreSqlContext().ResetDatabaseAsync();
    }


    public static void StopProgram()
    {
        Process.GetCurrentProcess().Kill();
    }


    public static void DbLog(string zaLog)
    {
    }


    private static async Task DoShitAsync()
    {
        foreach (var i in TypesFunction.GetDefaultObjectsAndSubclasses<Character>()
                     .Where(i => i is not Player)
                     .OrderByDescending(i => i.Rarity))
        {
            $"{i.Name}: {i.Rarity}".Print();
        }
        Process.GetCurrentProcess().Kill();
    }

    private static async Task Main(string[] args)
    {
        await DoShitAsync();
        if (args.Length > 0 && args[0] == "test-website")
        {
            await Website.StartAsync(args);
            return;
        }


        await using (var ctx = new PostgreSqlContext())
        {
            var stopwatch = new Stopwatch();
            Console.WriteLine("Making all users unoccupied...");
            stopwatch.Start();
            await ctx
                .Set<UserData>()
                .ExecuteUpdateAsync(i => i.SetProperty(j => j.IsOccupied, _ => false));
            Console.WriteLine("made all users unoccupied!");
        }

        await DiscordBot.DiscordBot.StartDiscordBotAsync();

        await Website.StartAsync(args);
    }
}