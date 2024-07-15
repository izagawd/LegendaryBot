using DiscordBotNet.Database;
using DiscordBotNet.Database.Models;
using DiscordBotNet.Extensions;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;

namespace DiscordBotNet.LegendaryBot.command;

public abstract class GeneralCommandClass 
{





    public GeneralCommandClass()
    {
        DatabaseContext = new PostgreSqlContext();
    }


    public Task<bool> HandleIsOccupiedAsync(IEnumerable<UserData> userDatas, CommandContext context,
        string? occupiedText = null)
    {
        return HandleIsOccupiedAsync(userDatas.Select(i => i.Id),context, occupiedText);
    }
    
    public async Task<bool> HandleIsOccupiedAsync(IEnumerable<ulong> userDatas, CommandContext context, string? occupiedText = null)
    {
        var arrayLongs = userDatas.ToArray();
        if ((await DatabaseContext.UserData
                .AnyAsync(i => arrayLongs.Contains(i.Id) &&  i.IsOccupied)))
        {
            return true;
        }

        return false;
    }

    public  async Task AfterSlashExecutionAsync(CommandContext ctx)
    {

        if(_occupiedUserDatasIds.Count > 0)
        {
         
            await using var tempCtx = new PostgreSqlContext();
            await tempCtx.UserData
                .Where(i => _occupiedUserDatasIds.Contains(i.Id))
                .ForEachAsync(i => i.IsOccupied = false);
            await tempCtx.SaveChangesAsync();
 
        }
        await DatabaseContext.DisposeAsync();
        DatabaseContext = null!;

    }

    ~GeneralCommandClass()
    {
        if(DatabaseContext is not null)
            DatabaseContext.Dispose();
    }
    private List<ulong> _occupiedUserDatasIds  = new();


    public IEnumerable<ulong> OccupiedUserDataIds
    {
        get
        {
            foreach (var i in _occupiedUserDatasIds)
            {
                yield return i;
            }
        }
    }
 
    
    protected async Task MakeOccupiedAsync(params ulong[] userDataIds)
    {

        await using var tempCtx = new PostgreSqlContext();
        await tempCtx.UserData
            .Where(i => userDataIds.Contains(i.Id))
            .ForEachAsync(i => i.IsOccupied = true);
        
        _occupiedUserDatasIds.AddRange(userDataIds);
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
    /// This exists cuz it's disposed at the end of a slash Commands and cuz I tend to forget to dispose disposable stuff
    /// </summary>
    public PostgreSqlContext DatabaseContext { get; private set; }


  
}