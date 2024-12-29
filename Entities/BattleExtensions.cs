using Entities.LegendaryBot.Entities.BattleEntities.Blessings;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Entities;

public static class BattleExtensions
{
    public static IIncludableQueryable<UserData, Blessing?> IncludeTeamWithBlessing
        (this IQueryable<UserData> queryable)
    {
        return queryable
            .Include(i => i.EquippedPlayerTeam!)
            .ThenInclude(i => i.TeamMemberships)
            .ThenInclude(i => i.Character)
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
            .ThenInclude(i => i!.TeamMemberships)
            .ThenInclude(i => i.Character)
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