using DiscordBotNet.Database.Models;

using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Blessings;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

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