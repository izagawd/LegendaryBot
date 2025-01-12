using System.Diagnostics;
using BasicFunctionality;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using DatabaseManagement;
using Entities.LegendaryBot.Entities.BattleEntities.Characters;
using Entities.LegendaryBot.Entities.Items;
using Entities.LegendaryBot.Entities.Items.ExpIncreaseMaterial;
using Entities.LegendaryBot.StatusEffects;
using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace WebsiteApi;

public static class Bot
{

    private static async Task DoShitAsync()
    {
        
        Console.WriteLine("yoo");
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
                .ExecuteUpdateAsync(i => i.SetProperty(j => j.IsOccupied,  false));
            Console.WriteLine("made all users unoccupied!");
        }

        await DiscordBot.DiscordBot.StartDiscordBotAsync();

        await Website.StartAsync(args);
    }
}