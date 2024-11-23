using System.Data;
using System.Linq.Expressions;
using BasicFunctionality;
using DatabaseManagement;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Commands;

public abstract class GeneralCommandClass
{

    private readonly List<long> _occupiedUserDatasIds = new();


    public GeneralCommandClass()
    {
        DatabaseContext = new PostgreSqlContext();
    }



    /// <summary>
    ///     This exists cuz it's disposed at the end of a slash Commands and cuz I tend to forget to dispose disposable stuff
    /// </summary>
    public PostgreSqlContext DatabaseContext { get; private set; }


    public async Task AskToDoBeginAsync(CommandContext context)
    {
        var embed = new DiscordEmbedBuilder()
            .WithTitle("Hmm")
            .WithDescription("You have not yet began your journey with /begin")
            .WithColor(TypesFunction.GetDefaultObject<UserData>().Color)
            .WithUser(context.User);

        var color = (await DatabaseContext.Set<UserData>()
                .Where(i => i.DiscordId == context.User.Id)
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

        var color = (await DatabaseContext.Set<UserData>()
                .Where(i => i.DiscordId == context.User.Id)
                .Select(i => new DiscordColor?(i.Color))
                .FirstOrDefaultAsync())
            .GetValueOrDefault(TypesFunction.GetDefaultObject<UserData>().Color);

        embed.WithColor(color);
        await context.RespondAsync(embed);
    }


    public async Task AfterExecutionAsync(CommandContext ctx)
    {
        try
        {
            if (_occupiedUserDatasIds.Count > 0)
            {
                await using var tempCtx = new PostgreSqlContext();
                await tempCtx.Set<UserData>()
                    .Where(i => _occupiedUserDatasIds.Contains(i.Id))
                    .ExecuteUpdateAsync(i
                        => i.SetProperty(j => j.IsOccupied, false));
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
     
            await DatabaseContext.DisposeAsync();
        }
 
   
    }



    /// <summary>
    ///     warning: Save changes will be called on database context
    /// </summary>
    /// <param name="userDataIds"></param>
    protected async Task MakeOccupiedAsync(params UserData[] userDatas)
    {
        var ids = userDatas.Select(i => i.Id).ToArray();
        _occupiedUserDatasIds.AddRange(ids);
        foreach (var i in userDatas) i.IsOccupied = true;
        var dic = userDatas.ToDictionary(i => i.Id, i => new { i, i.Version });
        await using var newDb = new PostgreSqlContext();
        var userDatasToUpdate = await newDb.Set<UserData>().Where(i => ids.Contains(i.Id))
            .ToArrayAsync();

        foreach (var i in userDatasToUpdate)
        {
            if (i.Version != dic[i.Id].Version) throw new DBConcurrencyException();

            i.IsOccupied = true;
        }

        await newDb.SaveChangesAsync();

        foreach (var i in userDatasToUpdate)
            if (dic.TryGetValue(i.Id, out var gotten))
            {
                gotten.i.Version = i.Version;
                DatabaseContext.Entry(gotten.i)
                    .Property(i => i.Version)
                    .OriginalValue = gotten.i.Version;
            }
    }


}