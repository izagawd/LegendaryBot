using System.Reflection;
using DiscordBotNet.Database.Models;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot;
using DiscordBotNet.LegendaryBot.Entities;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears.Stats;
using DiscordBotNet.LegendaryBot.Quests;
using Microsoft.EntityFrameworkCore;
using ConfigurationManager = System.Configuration.ConfigurationManager;


namespace DiscordBotNet.Database;

public class PostgreSqlContext : DbContext
{
    
    

    


    private static readonly Type[] EntityClasses;

    public DbSet<UserData> UserData { get; set; }
    public DbSet<GuildData> GuildData { get; set; }
    public DbSet<Entity> Entity { get; set; }


    public DbSet<Quote> Quote { get; set; }

    /// <summary>
    /// this should be called before any query if u want to ever use this method
    /// </summary>
    /// <param name="userId"> the user id u want  to refresh to a new day</param>
    public  async Task CheckForNewDayAsync(long userId)
    {

        var user = await UserData
            .Include(i => i.Quests)
            .FindOrCreateUserDataAsync(userId);
        var rightNowUtc = DateTime.UtcNow;
        if (user.LastTimeQuestWasChecked.Date == rightNowUtc.Date) return;
        
        user.Quests.Clear();
        var availableQuests = DefaultObjects.GetDefaultObjectsThatSubclass<Quest>()
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

   
 

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        
        // Configure the database provider and connection string
        optionsBuilder
            .UseNpgsql(ConfigurationManager.AppSettings["ConnectionString"])

            .EnableSensitiveDataLogging();
    
    }
    public void ResetDatabase()
    {
        Database.EnsureDeleted();
        Database.EnsureCreated();
      
    }
    public async Task ResetDatabaseAsync()
    {
        await Database.EnsureDeletedAsync();
        await Database.EnsureCreatedAsync();
      
    }



    private static readonly IEnumerable<Type> QuestTypes =
        Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(i => i.IsSubclassOf(typeof(Quest)) 
                        && !i.IsAbstract).ToArray();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UsePropertyAccessMode(PropertyAccessMode.Property);
        foreach (var i in DefaultObjects.AllAssemblyTypes.Where(i => i.IsRelatedToType(typeof(Quest))))
        {
            modelBuilder.Entity(i);
        }
        foreach (var entityType in DefaultObjects.AllAssemblyTypes.Where(i => i.IsRelatedToType(typeof(Entity))))
        {
            modelBuilder.Entity(entityType);
        }

        foreach (var i in DefaultObjects.AllAssemblyTypes.Where(i => i.IsRelatedToType(typeof(GearStat))))
        {
            modelBuilder.Entity(i);
        }
        foreach (var i in DefaultObjects.AllAssemblyTypes.Where(i => i.IsRelatedToType(typeof(Gear))))
        {
            modelBuilder.Entity(i);
        }
        modelBuilder
            .ApplyConfiguration(new UserDataDatabaseConfiguration())
            .ApplyConfiguration(new EntityDatabaseConfiguration())
            .ApplyConfiguration(new QuoteDatabaseConfiguration())
            .ApplyConfiguration(new PlayerTeamDatabaseConfiguration())
            .ApplyConfiguration(new CharacterDatabaseConfiguration())
            .ApplyConfiguration(new PlayerDatabaseConfiguration())
            .ApplyConfiguration(new GearDatabaseConfiguration())
            .ApplyConfiguration(new ArtifactStatConfiguration());






    }
}