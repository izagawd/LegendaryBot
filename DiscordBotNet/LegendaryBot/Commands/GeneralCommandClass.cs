using DiscordBotNet.Database;
using DiscordBotNet.Database.Models;
using DiscordBotNet.Extensions;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;

namespace DiscordBotNet.LegendaryBot.Commands;

public abstract class GeneralCommandClass 
{





    public GeneralCommandClass()
    {
        DatabaseContext = new PostgreSqlContext();
    }



    

    
    
    public async Task AskToDoBeginAsync(CommandContext context)
    {
        var embed = new DiscordEmbedBuilder()
            .WithTitle("Hmm")
            .WithDescription("You have not yet began your journey with /begin")
            .WithColor(TypesFunction.GetDefaultObject<UserData>().Color)
            .WithUser(context.User);

        var color = (await DatabaseContext.UserData
                .Where(i => i.Id == context.User.Id)
                .Select(i => new DiscordColor?(i.Color))
                .FirstOrDefaultAsync())
            .GetValueOrDefault(TypesFunction.GetDefaultObject<UserData>().Color);

        embed.WithColor(color);
        await context.RespondAsync(embed);
    }
    public async Task NotifyAboutOccupiedAsync(CommandContext context)
    {
        var embed = new DiscordEmbedBuilder()
            .WithTitle("Hmm")
            .WithDescription("You are occupied")
            .WithColor(TypesFunction.GetDefaultObject<UserData>().Color)
            .WithUser(context.User);

        var color = (await DatabaseContext.UserData
                .Where(i => i.Id == context.User.Id)
                .Select(i => new DiscordColor?(i.Color))
                .FirstOrDefaultAsync())
            .GetValueOrDefault(TypesFunction.GetDefaultObject<UserData>().Color);

        embed.WithColor(color);
        await context.RespondAsync(embed);
    }


 
    public  async Task AfterExecutionAsync(CommandContext ctx)
    {

        try
        {
            if(_occupiedUserDatasIds.Count > 0)
            {
         
                await using var tempCtx = new PostgreSqlContext();
                await tempCtx.UserData
                    .Where(i => _occupiedUserDatasIds.Contains(i.Id))
                    .ExecuteUpdateAsync(i
                        => i.SetProperty(j => j.IsOccupied, false));
            }
            await DatabaseContext.DisposeAsync();
            DatabaseContext = null!;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }


    }

    ~GeneralCommandClass()
    {
        DatabaseContext?.Dispose();
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
 
    
    /// <summary>
    /// warning: Save changes will be called on database context
    /// </summary>
    /// <param name="userDataIds"></param>
    protected async Task MakeOccupiedAsync(params ulong[] userDataIds)
    {
        _occupiedUserDatasIds.AddRange(userDataIds);
        await DatabaseContext.UserData
            .Where(i => userDataIds.Contains(i.Id))
            .ForEachAsync(i => i.IsOccupied = true);

        await DatabaseContext.SaveChangesAsync();
    }
    /// <summary>
    /// warning: Save changes will be called on database context
    /// </summary>
    /// <param name="userDataIds"></param>
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