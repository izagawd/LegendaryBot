using System.Reflection;
using DiscordBotNet.Database.ManyToManyInstances;
using DiscordBotNet.Database.Models;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot;
using DiscordBotNet.LegendaryBot.Entities;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Blessings;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears.Stats;
using DiscordBotNet.LegendaryBot.Quests;

using DSharpPlus.Entities;
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
        if (user.LastTimeChecked.Date == rightNowUtc.Date) return;
        
        user.Quests.Clear();
        var availableQuests = DefaultObjects.GetDefaultObjectsThatSubclass<Quest>()
            .Where(i => i.QuestTier == user.Tier)
            .Select(i => i.GetType())
            .ToList();

        while (user.Quests.Count < 4)
        {
            if(availableQuests.Count <= 0) break;
            var randomQuestType = BasicFunctionality.RandomChoice(availableQuests.AsEnumerable());
            availableQuests.Remove(randomQuestType);
            if (user.Quests.Any(j => j.GetType() == randomQuestType)) continue;
            user.Quests.Add((Quest) Activator.CreateInstance(randomQuestType)!);
            
        }

        
        user.LastTimeChecked = DateTime.UtcNow;


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