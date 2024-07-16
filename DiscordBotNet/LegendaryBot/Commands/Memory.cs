using System.Diagnostics;
using DSharpPlus.Commands;

namespace DiscordBotNet.LegendaryBot.Commands;
[Command("memory")]
public class Memory : GeneralCommandClass
{
    [Command("usage")]
    public async ValueTask ExecuteGetTotalMemoryUsedInBytes(CommandContext context)
    {
        var currentProcess = Process.GetCurrentProcess();
        
        // Get the memory usage in bytes
        var memoryUsageBytes = currentProcess.WorkingSet64;
        await context.RespondAsync($"Memory Usage: {memoryUsageBytes} bytes");

    }
}   