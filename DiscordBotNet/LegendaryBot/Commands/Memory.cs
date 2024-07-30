using System.Diagnostics;
using DSharpPlus.Commands;
using SixLabors.ImageSharp.Diagnostics;

namespace DiscordBotNet.LegendaryBot.Commands;
public class Memory : GeneralCommandClass
{
    [Command("memory-diagnostics")]
    public async ValueTask ExecuteGetTotalMemoryUsedInBytes(CommandContext context)
    {
        var currentProcess = Process.GetCurrentProcess();
        
        // Get the memory usage in bytes
        var memoryUsageBytes = currentProcess.WorkingSet64;
        await context.RespondAsync($"Memory Usage: {memoryUsageBytes} bytes" +
                                   $"\nTotal Undisposed Image Instances: {MemoryDiagnostics.TotalUndisposedAllocationCount}");

    }
}   