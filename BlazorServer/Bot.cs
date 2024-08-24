using System.Diagnostics;
using BlazorServer;
using DatabaseManagement;
using Microsoft.EntityFrameworkCore;

namespace DiscordBotNet;

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
        ;
    }

    private static async Task Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "test-website")
        {
            await Website.StartAsync(args);
            return;
        }

        await DoShitAsync();
        await using (var ctx = new PostgreSqlContext())
        {
            var stopwatch = new Stopwatch();
            Console.WriteLine("Making all users unoccupied...");
            stopwatch.Start();
            await ctx.UserData.ExecuteUpdateAsync(i => i.SetProperty(j => j.IsOccupied, _ => false));
            Console.WriteLine("made all users unoccupied!");
        }

        await DiscordBot.DiscordBot.StartDiscordBotAsync();

        await Website.StartAsync(args);
    }
}