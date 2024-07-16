using System.Linq.Expressions;
using System.Reflection;
using DiscordBotNet.Database.Models;

using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Blessings;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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
    public static void Reload(this CollectionEntry source)
    {
        if (source.CurrentValue != null)
        {
            foreach (var item in source.CurrentValue)
                source.EntityEntry.Context.Entry(item).State = EntityState.Detached;
            source.CurrentValue = null;
        }
        source.IsLoaded = false;
        source.Load();
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










}