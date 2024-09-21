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
        // Set the directory path where your images are stored
        string directoryPath = @"C:\Users\theiz\Downloads\Transparent"; 

        // Get all the files in the directory
        var files = Directory.GetFiles(directoryPath, "*-fococlipping-HD*");

        foreach (var file in files)
        {
            // Get the file name without the directory path
            string fileName = Path.GetFileName(file);

            // Remove the unwanted part from the file name
            string newFileName = fileName.Replace("-fococlipping-HD", "");

            // Combine the directory path and the new file name
            string newFilePath = Path.Combine(directoryPath, newFileName);

            try
            {
                // Rename the file
                File.Move(file, newFilePath);
                Console.WriteLine($"Renamed: {fileName} -> {newFileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to rename {fileName}: {ex.Message}");
            }
        }

        Console.WriteLine("Renaming process completed.");
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