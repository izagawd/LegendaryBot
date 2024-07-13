using System.Diagnostics;
using DSharpPlus.SlashCommands;

namespace DiscordBotNet.LegendaryBot.command;
[SlashCommandGroup("memory","commands for memory consumptiom")]
public class Memory : GeneralCommandClass
{
    [SlashCommand("usage","gets memory usage in bytes")]
    public async Task ExecuteGetTotalMemoryUsedInBytes(InteractionContext context)
    {
        Process currentProcess = Process.GetCurrentProcess();
        
        // Get the memory usage in bytes
        long memoryUsageBytes = currentProcess.WorkingSet64;
        await context.CreateResponseAsync($"Memory Usage: {memoryUsageBytes} bytes");

    }
}   