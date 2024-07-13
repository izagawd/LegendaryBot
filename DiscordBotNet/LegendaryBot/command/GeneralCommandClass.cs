using System.Reflection;
using DiscordBotNet.Database;
using DiscordBotNet.Database.Models;
using DSharpPlus.SlashCommands;
using Microsoft.EntityFrameworkCore;

namespace DiscordBotNet.LegendaryBot.command;

public abstract class GeneralCommandClass : ApplicationCommandModule
{

 


    public override Task<bool> BeforeSlashExecutionAsync(InteractionContext ctx)
    {
        DatabaseContext = new PostgreSqlContext();
        
        
        return Task.FromResult(true);
    }

    public override async Task AfterSlashExecutionAsync(InteractionContext ctx)
    {

        if(OccupiedUserDatasIds.Count > 0)
        {
         
            await using var tempCtx = new PostgreSqlContext();
            await tempCtx.UserData
                .Where(i => OccupiedUserDatasIds.Contains(i.Id))
                .ForEachAsync(i => i.IsOccupied = false);
            await tempCtx.SaveChangesAsync();
 
        }
        await DatabaseContext.DisposeAsync();
    }

    private List<long> OccupiedUserDatasIds { get; } = new();

    protected async Task MakeOccupiedAsync(params long[] userDataIds)
    {

        await using var tempCtx = new PostgreSqlContext();
        await tempCtx.UserData
            .Where(i => userDataIds.Contains(i.Id))
            .ForEachAsync(i => i.IsOccupied = true);
        
        OccupiedUserDatasIds.AddRange(userDataIds);
        await tempCtx.SaveChangesAsync();
    }
    protected Task MakeOccupiedAsync(params UserData[] userDatas)
    {
        foreach (var i in userDatas)
        {
            i.IsOccupied = true;
        }

        return MakeOccupiedAsync(userDatas.Select(i => i.Id).ToArray());
    }
    /// <summary>
    /// This exists cuz it's disposed at the end of a slash command and cuz I tend to forget to dispose disposable stuff
    /// </summary>
    protected PostgreSqlContext DatabaseContext { get; private set; }



    protected GeneralCommandClass()
    {
        DatabaseContext = null!;
    }
}