using System.Linq.Expressions;
using System.Reflection;
using DiscordBotNet.Database.Models;

using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Blessings;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace DiscordBotNet.Extensions;

public static class PostgreExtension
{
    public static Task<T?> RandomOrDefaultAsync<T>(this IQueryable<T> queryable)
    {
       return queryable.OrderBy(i => EF.Functions.Random()).FirstOrDefaultAsync();
    }

    public static T? RandomOrDefault<T>(this IQueryable<T> queryable)
    {
        return queryable.OrderBy(i => EF.Functions.Random()).FirstOrDefault();
    }

    public static Task<T> RandomAsync<T>(this IQueryable<T> queryable)
    {
        return queryable.OrderBy(i => EF.Functions.Random()).FirstAsync();
    }
    public static T Random<T>(this IQueryable<T> queryable)
    {
        return queryable.OrderBy(i => EF.Functions.Random()).First();
    }

    public static DbContext GetDbContext(this IQueryable query) 
    {

        
        var bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;
        var queryCompiler = typeof(EntityQueryProvider).GetField("_queryCompiler", bindingFlags)!.GetValue(query.Provider);
        var queryContextFactory = queryCompiler.GetType().GetField("_queryContextFactory", bindingFlags).GetValue(queryCompiler);

        var dependencies = typeof(RelationalQueryContextFactory).GetProperty("Dependencies", bindingFlags)!.GetValue(queryContextFactory);
        var queryContextDependencies = typeof(DbContext).Assembly.GetType(typeof(QueryContextDependencies).FullName!);
        var stateManagerProperty = queryContextDependencies!.GetProperty("StateManager", bindingFlags | BindingFlags.Public)!.GetValue(dependencies);
        var stateManager = (IStateManager)stateManagerProperty!;
        
        return  stateManager.Context;
    }

    public static T GetDbContext<T>(this IQueryable query) where T : DbContext
    {
        return (T) GetDbContext(query);
    }

    public static IQueryable<UserData> IncludeTeam
        (this IQueryable<UserData> queryable)
    {
        return queryable
            .Include(i => i.EquippedPlayerTeam);
    }
    
    public static IIncludableQueryable<UserData,Blessing?> IncludeTeamWithBlessing
        (this IQueryable<UserData> queryable)
    {
        return queryable
            .Include(i => i.EquippedPlayerTeam)
            .ThenInclude(i => i!.Characters)
            .ThenInclude(i => i.Blessing);
    }

    /// <summary>
    /// Note: this also includes their gear stats
    /// </summary>

    public static IQueryable<UserData> IncludeTeamWithGears
        (this IQueryable<UserData> queryable)
    {
        return queryable
            .Include(i => i.EquippedPlayerTeam)
            .ThenInclude(i => i!.Characters)
            .ThenInclude(i => i.Gears)
            .ThenInclude(i => i.Stats);
    }

    public static IQueryable<UserData> IncludeTeamWithAllEquipments
        (this IQueryable<UserData> queryable)
    {

        return queryable
            .IncludeTeamWithBlessing()
            .IncludeTeamWithGears();

    }



    /// <summary>
    /// this not only includes all the gears of the characters, but the stats as well
    /// </summary>

 







    public async static Task<TExpression> FindOrCreateSelectUserDataAsync<T, TExpression>(this IQueryable<T> queryable, long id,
         Expression<Func<T,TExpression>> selectExpression) where T : UserData,new()
    {
        TExpression? data = await queryable
                .Where(i => i.Id == id)
                .Select(selectExpression)
                .FirstOrDefaultAsync();
        

        if (data is null)
        {
            var tempData = new T { Id = id };
            await queryable.GetDbContext().AddRangeAsync(tempData);
            data = tempData.Map(selectExpression);
        }
        return data;
    }

    /// <summary>
    /// Gets a row/instance from the database. if not found, returns a new row/instance and adds it to the database when the DatabaseContext of this dbset is saved
    /// </summary>
    /// <param name="set"></param>
    /// <param name="id">The Id of the row/instance</param>
    /// <param name="includableQueryableFunc">If you want to include some shit use this</param>
    /// <typeparam name="T">The instance/row type</typeparam>
    /// <returns></returns>
    public async static Task<T> FindOrCreateUserDataAsync<T>(this IQueryable<T> set, long id) 
        where T : UserData, new()
    {
        T? data = await set
                .FirstOrDefaultAsync(i => i.Id == id);
     

        if (data is null)
        {
            data = new T { Id = id };
            await set.GetDbContext().Set<T>().AddAsync(data);
        }
        return data;
    }

}