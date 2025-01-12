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


    public static void StopProgram()
    {
        Process.GetCurrentProcess().Kill();
    }

    

    [MemoryDiagnoser(false)]
    public class ToBench
    {
        public static object kk;

        struct kk2
        {
            public unsafe fixed byte kk[30];
        }
        class Bruh
        {
            private kk2 k;
        }
        [Benchmark]
        public void ToBenchh()
        {
            kk = new Bruh();
        }
    }


    public static void DbLog(string zaLog)
    {
    }


    private static async Task DoShitAsync()
    {
        

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