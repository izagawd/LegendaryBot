using DiscordBotNet.Extensions;
using DSharpPlus.Commands;

namespace DiscordBotNet.LegendaryBot.Commands;

public class FarmGear : GeneralCommandClass
{
    [Command("idk")]
    public async ValueTask ExecuteAsync(CommandContext context, string yo, string bruh)
    {
        $"It is {yo}".Print();
        $"And it is {bruh}".Print();
    }
}   