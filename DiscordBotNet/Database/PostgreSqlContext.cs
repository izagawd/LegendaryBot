using DiscordBotNet.Database.Models;
using DiscordBotNet.LegendaryBot;
using DiscordBotNet.LegendaryBot.Entities;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Blessings;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears.Stats;
using DiscordBotNet.LegendaryBot.Entities.Items;
using DiscordBotNet.LegendaryBot.Quests;
using Microsoft.EntityFrameworkCore;
using ConfigurationManager = System.Configuration.ConfigurationManager;


namespace DiscordBotNet.Database;



public class PostgreSqlContext : DbContext
{
    
    

    



    public DbSet<UserData> UserData { get; set; }
    public DbSet<GuildData> GuildDatas { get; set; }
    public DbSet<Blessing> Blessings { get; set; }

    public DbSet<Gear> Gears { get; set; }
    public DbSet<Character> Characters { get; set; }
    public DbSet<Item> Items { get; set; }
    public DbSet<Quote> Quote { get; set; }

    /// <summary>
    /// this should be called before any query if u want to ever use this method
    /// </summary>
    /// <param name="userId"> the user id u want  to refresh to a new day</param>
    public  async Task CheckForNewDayAsync(ulong userId)
    {

        var user = await UserData
            .Include(i => i.Quests)
            .FirstOrDefaultAsync(i => i.Id == userId);
        if(user is null)
            return;
        var rightNowUtc = DateTime.UtcNow;
        if (user.LastTimeQuestWasChecked.Date == rightNowUtc.Date) return;
        
        user.Quests.Clear();
        var availableQuests = TypesFunctionality.GetDefaultObjectsThatIsInstanceOf<Quest>()
            .Select(i => i.GetType())
            .Where(i => !i.IsAbstract)
            .OrderBy(_ => BasicFunctionality.GetRandomNumberInBetween(0, 100))
            .Take(4);

        foreach (var i in availableQuests)
        {
            user.Quests.Add((Quest) Activator.CreateInstance(i)!);
        }
   

        
        user.LastTimeQuestWasChecked = DateTime.UtcNow;


    }

    public async Task<UserData> CreateNonExistantUserdataAsync(ulong id)
    {
        var user = new UserData() { Id = id };
        await UserData.AddAsync(user);
        return user;
    }
 

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        
        // Configure the database provider and connection string
        optionsBuilder
            .UseNpgsql(ConfigurationManager.AppSettings[Bot.DatabaseUrlPathToUse])

            .EnableSensitiveDataLogging();
    
    }

    /// <param name="tableName">WARNING: case insensitive</param>
    /// <param name="userDataIdColumnName">case sensitive</param>
    /// <param name="numberColumnName">case sensitive</param>
    private async Task SetupNumberIncrementorForAsync(string tableName, string userDataIdColumnName ,
        string numberColumnName )
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
        if (!Bot.UseTestDatabaseAndBot)
        {
            throw new Exception("Cannot reset database. you are on the main server right now");
        }
        await Database.EnsureDeletedAsync();
        await Database.EnsureCreatedAsync();
        await SetupDatabaseTriggersAsync();

    }

    private async Task SetupDatabaseTriggersAsync()
    {
        await SetupNumberIncrementorForGearAsync();
        await SetupNumberIncrementorForCharacterAsync();

    }

    private Task SetupNumberIncrementorForCharacterAsync()
    {
        return SetupNumberIncrementorForAsync(nameof(Characters),
            nameof(Character.UserDataId),nameof(Character.Number));
    }
    private Task SetupNumberIncrementorForGearAsync()
    {
        return SetupNumberIncrementorForAsync(nameof(Gears),nameof(Gear.UserDataId),
            nameof(Gear.Number));
    }
    


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UsePropertyAccessMode(PropertyAccessMode.Property);
        foreach (var i in TypesFunctionality.AllAssemblyTypes.Where(i => i.IsAssignableTo(typeof(Quest))))
        {
            modelBuilder.Entity(i);
        }
        foreach (var entityType in TypesFunctionality
                     .AllAssemblyTypes
                     .Where(i => i.IsClass && i.GetInterfaces().Contains(typeof(IInventoryEntity))))
        {
            modelBuilder.Entity(entityType);
        }
        foreach (var i in TypesFunctionality.AllAssemblyTypes.Where(i => i.IsAssignableTo(typeof(GearStat))))
        {
            modelBuilder.Entity(i);
        }
        modelBuilder
            .ApplyConfiguration(new ItemDatabaseConfiguration())
            .ApplyConfiguration(new UserDataDatabaseConfiguration())
            .ApplyConfiguration(new QuoteDatabaseConfiguration())
            .ApplyConfiguration(new PlayerTeamDatabaseConfiguration())
            .ApplyConfiguration(new CharacterDatabaseConfiguration())
            .ApplyConfiguration(new PlayerDatabaseConfiguration())
            .ApplyConfiguration(new GearDatabaseConfiguration())
            .ApplyConfiguration(new GearStatDatabaseConfiguration())
            .ApplyConfiguration(new BlessingDatabaseConfiguration())
            .ApplyConfiguration(new GuildDataDatabaseConfig());
    }
}