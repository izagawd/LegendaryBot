using BasicFunctionality;
using Entities.LegendaryBot;
using Entities.LegendaryBot.Entities;
using Entities.LegendaryBot.Entities.BattleEntities.Blessings;
using Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using Entities.LegendaryBot.Entities.BattleEntities.Gears;
using Entities.LegendaryBot.Entities.BattleEntities.Gears.Stats;
using Entities.LegendaryBot.Entities.Items;
using Entities.LegendaryBot.Quests;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using PublicInfo;
using ConfigurationManager = System.Configuration.ConfigurationManager;


namespace DatabaseManagement;

public class PostgreSqlContext : DbContext
{
    public const string DatabaseUrlPathToUse = Information.IsTesting ? "LocalConnectionString" : "ConnectionString";


    /// <summary>
    ///     this should be called before any query if u want to ever use this method
    /// </summary>
    /// <param name="discordUserId"> the user id u want  to refresh to a new day</param>
    public async Task CheckForNewDayAsync(ulong discordUserId)
    {
        var user = await Set<UserData>()
            .Include(i => i.Quests)
            .FirstOrDefaultAsync(i => i.DiscordId == discordUserId);
        if (user is null)
            return;
        var rightNowUtc = DateTime.UtcNow;
        if (user.LastTimeQuestWasChecked.Date == rightNowUtc.Date) return;
        user.Quests.Clear();
        var availableQuests = TypesFunction.GetDefaultObjectsAndSubclasses<Quest>()
            .Select(i => i.GetType())
            .Where(i => !i.IsAbstract)
            .OrderBy(_ => BasicFunctions.GetRandomNumberInBetween(0, 100))
            .Take(4);

        foreach (var i in availableQuests) user.Quests.Add((Quest)Activator.CreateInstance(i)!);

        user.LastTimeQuestWasChecked = DateTime.UtcNow;
    }

    public async Task<UserData> CreateNonExistantUserdataAsync(ulong id)
    {
        var user = new UserData { DiscordId = id };
        await Set<UserData>().AddAsync(user);
        return user;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Configure the database provider and connection string
        optionsBuilder
            .UseNpgsql(ConfigurationManager.AppSettings["ConnectionString"])
            .EnableSensitiveDataLogging();
    }

    /// <param name="tableName">WARNING: case insensitive</param>
    /// <param name="userDataIdColumnName">case sensitive</param>
    /// <param name="numberColumnName">case sensitive</param>
    private async Task SetupNumberIncrementorForAsync(string tableName, string userDataIdColumnName,
        string numberColumnName)
    {
        var functionName = $"set_number_for_new_{tableName.ToLower()}_row";
#pragma warning disable EF1002
        await Database.ExecuteSqlRawAsync(@$"
CREATE OR REPLACE FUNCTION {functionName}()
RETURNS TRIGGER AS $$
BEGIN
    -- Set the Number to the maximum Number + 1 where UserDataId matches
    NEW.""{numberColumnName}"" := COALESCE(
        (SELECT COALESCE(MAX(""{numberColumnName}""), 0) + 1
         FROM ""{tableName}""
         WHERE ""{userDataIdColumnName}"" = NEW.""{userDataIdColumnName}""),
       1                                   
    );
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;
   CREATE OR REPLACE TRIGGER {functionName}_trigger
BEFORE INSERT ON ""{tableName}""
FOR EACH ROW
EXECUTE FUNCTION {functionName}();
");
#pragma warning restore EF1002
    }

    public async Task ResetDatabaseAsync()
    {
        if (!Information.IsTesting)
            throw new Exception("Cannot reset database. you are on the main server right now");
        await Database.EnsureDeletedAsync();
        await Database.EnsureCreatedAsync();
        await SetupDatabaseTriggersAsync();
    }

    private async Task SetupDatabaseTriggersAsync()
    {
        await SetupNumberIncrementorForGearAsync();

    }


    private Task SetupNumberIncrementorForGearAsync()
    {
        return SetupNumberIncrementorForAsync(nameof(Gear), nameof(Gear.UserDataId),
            nameof(Gear.Number));
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UsePropertyAccessMode(PropertyAccessMode.Property);
        foreach (var i in TypesFunction.AllTypes.Where(i => i.IsAssignableTo(typeof(Quest)))) modelBuilder.Entity(i);
        foreach (var entityType in TypesFunction
                     .AllTypes
                     .Where(i => i.IsClass && i.IsAssignableTo(typeof(IInventoryEntity))))
            modelBuilder.Entity(entityType);
        foreach (var i in TypesFunction.AllTypes.Where(i => i.IsAssignableTo(typeof(GearStat)))) modelBuilder.Entity(i);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserData).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PostgreSqlContext).Assembly);
    }
}